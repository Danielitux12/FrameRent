namespace FrameRent.Application.DTOs.Equipment;

public record EquipmentResponse(
    Guid Id,
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalUnits,
    bool IsActive
);
