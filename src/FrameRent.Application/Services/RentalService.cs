using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Domain.ValueObjects;

namespace FrameRent.Application.Services;

/// <summary>
/// Implements rental lifecycle operations, enforcing authorization, inventory constraints,
/// client quota limits, and domain aggregate state transitions.
/// </summary>
public class RentalService : IRentalService
{
    private const int MaxActiveRentalsPerClient = 3;

    private readonly IRentalRepository _rentalRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RentalService(
        IRentalRepository rentalRepository,
        IEquipmentRepository equipmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _rentalRepository = rentalRepository;
        _equipmentRepository = equipmentRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<RentalResponse> CreateAsync(CreateRentalRequest request, CancellationToken ct = default)
    {
        var clientId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ForbiddenAccessException("Authentication is required.");
        }

        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), request.EquipmentId);
        }

        if (!equipment.IsActive)
        {
            throw new ValidationAppException("This equipment is not available for rental.");
        }

        var activeCount = await _rentalRepository.CountActiveForClientAsync(clientId, ct);
        if (activeCount >= MaxActiveRentalsPerClient)
        {
            throw new ValidationAppException($"Client has reached the maximum allowed active rentals limit ({MaxActiveRentalsPerClient}).");
        }

        var dateRange = DateRange.Create(request.StartDate, request.EndDate);

        var blockingRentals = await _rentalRepository.GetBlockingRentalsForEquipmentAsync(equipment.Id, ct);
        var committed = blockingRentals
            .Where(r => r.Period.Overlaps(dateRange))
            .Sum(r => r.Quantity);

        if (committed + request.Quantity > equipment.TotalUnits)
        {
            throw new ValidationAppException($"Not enough available units for '{equipment.Name}' in the selected date range.");
        }

        var rental = Rental.Create(equipment.Id, clientId, request.Quantity, dateRange, equipment.DailyRate);

        await _rentalRepository.AddAsync(rental, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, equipment.Name);
    }

    public async Task<List<RentalResponse>> GetMineAsync(CancellationToken ct = default)
    {
        var clientId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ForbiddenAccessException("Authentication is required.");
        }

        var rentals = await _rentalRepository.GetByClientIdAsync(clientId, ct);
        return rentals.Select(r => ToResponse(r, r.Equipment?.Name ?? string.Empty)).ToList();
    }

    public async Task<RentalResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        if (!_currentUser.IsInRole(Roles.Staff) && rental.ClientId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only view your own rentals.");
        }

        return ToResponse(rental, rental.Equipment?.Name ?? string.Empty);
    }

    public async Task<List<RentalResponse>> GetAllAsync(RentalStatus? status, CancellationToken ct = default)
    {
        var rentals = await _rentalRepository.GetAllAsync(status, ct);
        return rentals.Select(r => ToResponse(r, r.Equipment?.Name ?? string.Empty)).ToList();
    }

    public async Task<RentalResponse> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        if (rental.ClientId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only cancel your own rentals.");
        }

        rental.Cancel();

        _rentalRepository.Update(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, rental.Equipment?.Name ?? string.Empty);
    }

    public async Task<RentalResponse> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        var equipment = rental.Equipment ?? await _equipmentRepository.GetByIdAsync(rental.EquipmentId, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), rental.EquipmentId);
        }

        var blockingRentals = await _rentalRepository.GetBlockingRentalsForEquipmentAsync(equipment.Id, ct);
        var committed = blockingRentals
            .Where(r => r.Id != rental.Id && r.Period.Overlaps(rental.Period))
            .Sum(r => r.Quantity);

        if (committed + rental.Quantity > equipment.TotalUnits)
        {
            throw new ValidationAppException($"Not enough available units for '{equipment.Name}' in the selected date range.");
        }

        rental.Approve();

        _rentalRepository.Update(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, equipment.Name);
    }

    public async Task<RentalResponse> RejectAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        rental.Reject();

        _rentalRepository.Update(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, rental.Equipment?.Name ?? string.Empty);
    }

    public async Task<RentalResponse> DeliverAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        rental.Deliver();

        _rentalRepository.Update(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, rental.Equipment?.Name ?? string.Empty);
    }

    public async Task<RentalResponse> ReturnAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await _rentalRepository.GetByIdWithEquipmentAsync(id, ct);
        if (rental is null)
        {
            throw new NotFoundException(nameof(Rental), id);
        }

        var equipment = rental.Equipment ?? await _equipmentRepository.GetByIdAsync(rental.EquipmentId, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), rental.EquipmentId);
        }

        rental.Return(DateTime.UtcNow, equipment.DailyRate);

        _rentalRepository.Update(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rental, equipment.Name);
    }

    private static RentalResponse ToResponse(Rental rental, string equipmentName) =>
        new(
            rental.Id,
            rental.EquipmentId,
            equipmentName,
            rental.ClientId,
            rental.Status.ToString(),
            rental.Quantity,
            rental.Period.Start,
            rental.Period.End,
            rental.Cost.Total,
            rental.Cost.LateFee,
            rental.CreatedAt,
            rental.ApprovedAt,
            rental.DeliveredAt,
            rental.ReturnedAt,
            rental.CancelledAt,
            rental.RejectedAt
        );
}
