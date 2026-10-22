namespace FrameRent.Application.DTOs.Equipment;

public record AvailabilityResponse(
    Guid EquipmentId,
    DateTime From,
    DateTime To,
    int TotalUnits,
    int CommittedUnits,
    int AvailableUnits
);
