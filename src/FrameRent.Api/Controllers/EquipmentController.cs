using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

/// <summary>
/// Endpoints for equipment catalog, availability lookups, and inventory management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// Retrieves active equipment items available in the public catalog.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<EquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EquipmentResponse>>> GetCatalog(CancellationToken ct)
    {
        var result = await _equipmentService.GetActiveCatalogAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all equipment items, including inactive ones. Only accessible by Staff.
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(List<EquipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EquipmentResponse>>> GetAllForStaff(CancellationToken ct)
    {
        var result = await _equipmentService.GetAllForStaffAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves equipment details by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _equipmentService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Checks real-time unit availability for an equipment item across a date window.
    /// </summary>
    [HttpGet("{id:guid}/availability")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailability(
        Guid id,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var result = await _equipmentService.GetAvailabilityAsync(id, from, to, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new equipment item in inventory. Only accessible by Staff.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EquipmentResponse>> Create([FromBody] CreateEquipmentRequest request, CancellationToken ct)
    {
        var result = await _equipmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing equipment item. Only accessible by Staff.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EquipmentResponse>> Update(
        Guid id,
        [FromBody] UpdateEquipmentRequest request,
        CancellationToken ct)
    {
        var result = await _equipmentService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates an equipment item. Fails if active commitments exist. Only accessible by Staff.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _equipmentService.DeactivateAsync(id, ct);
        return NoContent();
    }
}
