using System.ComponentModel.DataAnnotations;

namespace FrameRent.Application.DTOs.Rentals;

public record CreateRentalRequest
{
    [Required]
    public Guid EquipmentId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; init; }

    [Required]
    public DateTime StartDate { get; init; }

    [Required]
    public DateTime EndDate { get; init; }
}
