namespace FrameRent.Application.DTOs.Rentals;

public record RentalResponse(
    Guid Id,
    Guid EquipmentId,
    string EquipmentName,
    string ClientId,
    string Status,
    int Quantity,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalCost,
    decimal? LateFee,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? DeliveredAt,
    DateTime? ReturnedAt,
    DateTime? CancelledAt,
    DateTime? RejectedAt
);
