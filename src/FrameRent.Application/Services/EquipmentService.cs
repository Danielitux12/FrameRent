using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

/// <summary>
/// Implements equipment inventory management, catalog queries, and availability evaluations.
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EquipmentService(
        IEquipmentRepository equipmentRepository,
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork)
    {
        _equipmentRepository = equipmentRepository;
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EquipmentResponse>> GetActiveCatalogAsync(CancellationToken ct = default)
    {
        var equipmentList = await _equipmentRepository.GetActiveAsync(ct);
        return equipmentList.Select(ToResponse).ToList();
    }

    public async Task<List<EquipmentResponse>> GetAllForStaffAsync(CancellationToken ct = default)
    {
        var equipmentList = await _equipmentRepository.GetAllAsync(ct);
        return equipmentList.Select(ToResponse).ToList();
    }

    public async Task<EquipmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), id);
        }

        return ToResponse(equipment);
    }

    public async Task<AvailabilityResponse> GetAvailabilityAsync(Guid id, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), id);
        }

        var startDate = from.Date;
        var endDate = to.Date;

        if (endDate <= startDate)
        {
            throw new ValidationAppException("End date must be strictly after start date.");
        }

        // DateRange.Create enforces constraints intended for reservation creation (e.g. start date not in past,
        // duration <= 15 days). However, this endpoint is a read-only availability query where clients or staff
        // might inspect arbitrary inquiry windows without creating a reservation contract.
        // Therefore, we check overlap directly using the exact interval logic of DateRange.Overlaps (Start < other.End && End > other.Start)
        // against the calendar dates without forcing DateRange factory validation here.
        var blockingRentals = await _rentalRepository.GetBlockingRentalsForEquipmentAsync(id, ct);
        var committed = blockingRentals
            .Where(r => r.Period.Start < endDate && r.Period.End > startDate)
            .Sum(r => r.Quantity);

        var available = Math.Max(0, equipment.TotalUnits - committed);

        return new AvailabilityResponse(
            equipment.Id,
            startDate,
            endDate,
            equipment.TotalUnits,
            committed,
            available
        );
    }

    public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken ct = default)
    {
        var equipment = Equipment.Create(
            request.Name,
            request.Category,
            request.Description ?? string.Empty,
            request.DailyRate,
            request.TotalUnits
        );

        await _equipmentRepository.AddAsync(equipment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(equipment);
    }

    public async Task<EquipmentResponse> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken ct = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), id);
        }

        equipment.Update(
            request.Name,
            request.Category,
            request.Description ?? string.Empty,
            request.DailyRate,
            request.TotalUnits
        );

        _equipmentRepository.Update(equipment);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(equipment);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, ct);
        if (equipment is null)
        {
            throw new NotFoundException(nameof(Equipment), id);
        }

        var hasActiveCommitments = await _rentalRepository.HasActiveCommitmentsAsync(id, ct);
        if (hasActiveCommitments)
        {
            throw new ValidationAppException("This equipment cannot be deactivated because it has Approved or Delivered rentals.");
        }

        equipment.Deactivate();

        _equipmentRepository.Update(equipment);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static EquipmentResponse ToResponse(Equipment equipment) =>
        new(
            equipment.Id,
            equipment.Name,
            equipment.Category,
            equipment.Description,
            equipment.DailyRate,
            equipment.TotalUnits,
            equipment.IsActive
        );
}
