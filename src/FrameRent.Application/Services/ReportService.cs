using FrameRent.Application.DTOs.Reports;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

/// <summary>
/// Implements reporting and equipment utilization analytics.
/// </summary>
public class ReportService : IReportService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReportService(
        IRentalRepository rentalRepository,
        IEquipmentRepository equipmentRepository)
    {
        _rentalRepository = rentalRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<List<TopEquipmentResponse>> GetTopEquipmentAsync(CancellationToken ct = default)
    {
        var mostRented = await _rentalRepository.GetMostRentedEquipmentAsync(ct);
        var allEquipment = await _equipmentRepository.GetAllAsync(ct);
        var equipmentMap = allEquipment.ToDictionary(e => e.Id);

        var reports = mostRented
            .Select(item =>
            {
                equipmentMap.TryGetValue(item.EquipmentId, out var eq);
                return new TopEquipmentResponse(
                    item.EquipmentId,
                    eq?.Name ?? "Unknown",
                    eq?.Category ?? "Unknown",
                    item.RentalCount
                );
            })
            .OrderByDescending(r => r.RentalCount)
            .ToList();

        return reports;
    }
}
