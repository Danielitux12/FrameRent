namespace FrameRent.Application.DTOs.Reports;

public record TopEquipmentResponse(
    Guid EquipmentId,
    string Name,
    string Category,
    int RentalCount
);
