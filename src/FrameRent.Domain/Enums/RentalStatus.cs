namespace FrameRent.Domain.Enums;

/// <summary>
/// Represents the lifecycle states of a rental in the domain.
/// </summary>
public enum RentalStatus
{
    Pending,
    Approved,
    Delivered,
    Returned,
    Rejected,
    Cancelled
}
