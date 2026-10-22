using FrameRent.Application.DTOs.Reports;

namespace FrameRent.Application.Interfaces;

/// <summary>
/// Contract for analytical reports and equipment utilization metrics.
/// </summary>
public interface IReportService
{
    Task<List<TopEquipmentResponse>> GetTopEquipmentAsync(CancellationToken ct = default);
}
