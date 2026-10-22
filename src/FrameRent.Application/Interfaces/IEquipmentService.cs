using FrameRent.Application.DTOs.Equipment;

namespace FrameRent.Application.Interfaces;

/// <summary>
/// Contract for equipment catalog management and stock availability queries.
/// </summary>
public interface IEquipmentService
{
    Task<List<EquipmentResponse>> GetActiveCatalogAsync(CancellationToken ct = default);
    Task<List<EquipmentResponse>> GetAllForStaffAsync(CancellationToken ct = default);
    Task<EquipmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AvailabilityResponse> GetAvailabilityAsync(Guid id, DateTime from, DateTime to, CancellationToken ct = default);
    Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken ct = default);
    Task<EquipmentResponse> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
