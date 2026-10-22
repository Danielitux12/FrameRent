using FrameRent.Domain.Enums;
using FrameRent.Domain.Exceptions;
using FrameRent.Domain.ValueObjects;

namespace FrameRent.Domain.Entities;

/// <summary>
/// Represents a rental reservation in the domain.
/// Acts as the Aggregate Root for managing the lifecycle, state transitions, and cost tracking of a rental.
/// </summary>
public class Rental
{
    public Guid Id { get; private set; }
    public Guid EquipmentId { get; private set; }
    public Equipment? Equipment { get; private set; }
    public string ClientId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public RentalStatus Status { get; private set; }
    public RentalCost Cost { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    /// <summary>
    /// Indicates whether this rental reserves equipment inventory units, blocking availability.
    /// True when Approved or Delivered.
    /// </summary>
    public bool BlocksAvailability => Status is RentalStatus.Approved or RentalStatus.Delivered;

    /// <summary>
    /// Indicates whether this rental counts towards the client's concurrent active rentals limit.
    /// True when Pending or Approved.
    /// </summary>
    public bool CountsTowardClientLimit => Status is RentalStatus.Pending or RentalStatus.Approved;

    // Parameterless constructor required by EF Core
    protected Rental() { }

    private Rental(
        Guid id,
        Guid equipmentId,
        string clientId,
        int quantity,
        DateRange period,
        RentalCost cost,
        DateTime createdAt)
    {
        Id = id;
        EquipmentId = equipmentId;
        ClientId = clientId;
        Quantity = quantity;
        Period = period;
        Cost = cost;
        Status = RentalStatus.Pending;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory method to create a new rental reservation in Pending status.
    /// </summary>
    public static Rental Create(Guid equipmentId, string clientId, int quantity, DateRange period, decimal dailyRate)
    {
        if (equipmentId == Guid.Empty)
        {
            throw new DomainException("Equipment ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new DomainException("Client ID cannot be empty or whitespace.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (period is null)
        {
            throw new DomainException("Rental period cannot be null.");
        }

        var cost = RentalCost.Calculate(dailyRate, quantity, period);

        return new Rental(
            Guid.NewGuid(),
            equipmentId,
            clientId.Trim(),
            quantity,
            period,
            cost,
            DateTime.UtcNow
        );
    }

    /// <summary>
    /// Approves the pending rental reservation.
    /// </summary>
    public void Approve()
    {
        if (Status != RentalStatus.Pending)
        {
            throw new DomainException($"Cannot approve rental. Current status is '{Status}', but only '{RentalStatus.Pending}' rentals can be approved.");
        }

        Status = RentalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the pending rental reservation.
    /// </summary>
    public void Reject()
    {
        if (Status != RentalStatus.Pending)
        {
            throw new DomainException($"Cannot reject rental. Current status is '{Status}', but only '{RentalStatus.Pending}' rentals can be rejected.");
        }

        Status = RentalStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Delivers the approved equipment to the client.
    /// </summary>
    public void Deliver()
    {
        if (Status != RentalStatus.Approved)
        {
            throw new DomainException($"Cannot deliver rental. Current status is '{Status}', but only '{RentalStatus.Approved}' rentals can be delivered.");
        }

        Status = RentalStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns the delivered equipment and recalculates costs including any applicable late fees.
    /// </summary>
    public void Return(DateTime returnDate, decimal dailyRate)
    {
        if (Status != RentalStatus.Delivered)
        {
            throw new DomainException($"Cannot return rental. Current status is '{Status}', but only '{RentalStatus.Delivered}' rentals can be returned.");
        }

        var lateFee = RentalCost.CalculateLateFee(returnDate, Period.End, dailyRate, Quantity);
        Cost = Cost.WithLateFee(lateFee);

        Status = RentalStatus.Returned;
        ReturnedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the rental reservation if it is Pending or Approved and before the rental start date.
    /// </summary>
    public void Cancel()
    {
        if (Status != RentalStatus.Pending && Status != RentalStatus.Approved)
        {
            throw new DomainException($"Cannot cancel rental. Current status is '{Status}', but only '{RentalStatus.Pending}' or '{RentalStatus.Approved}' rentals can be cancelled.");
        }

        var today = DateTime.UtcNow.Date;
        if (today >= Period.Start)
        {
            throw new DomainException($"Cannot cancel rental. Cancellation is only allowed strictly before the rental start date ({Period.Start:yyyy-MM-dd}). Current date is {today:yyyy-MM-dd}.");
        }

        Status = RentalStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }
}
