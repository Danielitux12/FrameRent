using FrameRent.Application.DTOs.Rentals;
using FrameRent.Domain.Enums;

namespace FrameRent.Application.Interfaces;

/// <summary>
/// Contract for rental lifecycle management, reservation orchestration, and client queries.
/// </summary>
public interface IRentalService
{
    Task<RentalResponse> CreateAsync(CreateRentalRequest request, CancellationToken ct = default);
    Task<List<RentalResponse>> GetMineAsync(CancellationToken ct = default);
    Task<RentalResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RentalResponse>> GetAllAsync(RentalStatus? status, CancellationToken ct = default);
    Task<RentalResponse> CancelAsync(Guid id, CancellationToken ct = default);
    Task<RentalResponse> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<RentalResponse> RejectAsync(Guid id, CancellationToken ct = default);
    Task<RentalResponse> DeliverAsync(Guid id, CancellationToken ct = default);
    Task<RentalResponse> ReturnAsync(Guid id, CancellationToken ct = default);
}
