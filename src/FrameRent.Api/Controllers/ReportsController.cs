using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Reports;
using FrameRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

/// <summary>
/// Endpoints for management metrics and business reporting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Staff)]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Retrieves the most rented equipment rankings. Only accessible by Staff.
    /// </summary>
    [HttpGet("top-equipment")]
    [ProducesResponseType(typeof(List<TopEquipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<TopEquipmentResponse>>> GetTopEquipment(CancellationToken ct)
    {
        var result = await _reportService.GetTopEquipmentAsync(ct);
        return Ok(result);
    }
}
