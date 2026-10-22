using System.ComponentModel.DataAnnotations;

namespace FrameRent.Application.DTOs.Equipment;

public record CreateEquipmentRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Category { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Daily rate must be greater than zero.")]
    public decimal DailyRate { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Total units must be at least 1.")]
    public int TotalUnits { get; init; }
}
