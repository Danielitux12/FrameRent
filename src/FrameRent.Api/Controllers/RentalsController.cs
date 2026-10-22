using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

/// <summary>
/// Endpoints for rental reservation lifecycle management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    /// <summary>
    /// Creates a new rental request for the authenticated client.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Client)]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RentalResponse>> Create([FromBody] CreateRentalRequest request, CancellationToken ct)
    {
        var result = await _rentalService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves all rentals belonging to the authenticated client.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Client)]
    [ProducesResponseType(typeof(List<RentalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RentalResponse>>> GetMine(CancellationToken ct)
    {
        var result = await _rentalService.GetMineAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all rentals across all clients, optionally filtered by status. Only accessible by Staff.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(List<RentalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RentalResponse>>> GetAll([FromQuery] RentalStatus? status, CancellationToken ct)
    {
        var result = await _rentalService.GetAllAsync(status, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves rental details by identifier. Clients can only see their own rentals; Staff can see any.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Cancels a rental prior to the start date. Only owner client can cancel.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.CancelAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Approves a pending rental. Only accessible by Staff.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> Approve(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.ApproveAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a pending rental. Only accessible by Staff.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> Reject(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.RejectAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Marks an approved rental as delivered to client. Only accessible by Staff.
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> Deliver(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.DeliverAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Marks a delivered rental as returned and calculates late fees if applicable. Only accessible by Staff.
    /// </summary>
    [HttpPost("{id:guid}/return")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> Return(Guid id, CancellationToken ct)
    {
        var result = await _rentalService.ReturnAsync(id, ct);
        return Ok(result);
    }
}
