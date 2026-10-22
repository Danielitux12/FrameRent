using FrameRent.Domain.Exceptions;

namespace FrameRent.Domain.ValueObjects;

/// <summary>
/// Represents an immutable rental financial breakdown, including standard rental charge and potential late fee penalties.
/// </summary>
public sealed record RentalCost
{
    /// <summary>
    /// Penalty multiplier applied to the daily rate for each day past the agreed return date (150% rate).
    /// </summary>
    public const decimal LateFeeMultiplier = 1.5m;

    public decimal Total { get; }
    public decimal? LateFee { get; }

    private RentalCost() { }

    private RentalCost(decimal total, decimal? lateFee = null)
    {
        Total = total;
        LateFee = lateFee;
    }

    /// <summary>
    /// Calculates the initial rental cost based on daily rate, requested quantity, and duration.
    /// </summary>
    /// <param name="dailyRate">The daily rental rate per equipment unit.</param>
    /// <param name="quantity">The number of units reserved.</param>
    /// <param name="range">The agreed rental period.</param>
    /// <returns>A new <see cref="RentalCost"/> with calculated Total and null LateFee.</returns>
    /// <exception cref="DomainException">Thrown if parameters are non-positive or range is null.</exception>
    public static RentalCost Calculate(decimal dailyRate, int quantity, DateRange range)
    {
        if (range is null)
        {
            throw new DomainException("Date range is required to calculate rental cost.");
        }

        if (dailyRate <= 0)
        {
            throw new DomainException("Daily rate must be greater than zero.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        var total = range.DurationDays * dailyRate * quantity;
        return new RentalCost(total, null);
    }

    /// <summary>
    /// Calculates the late fee penalty incurred if equipment is returned after the agreed end date.
    /// Penalty formula: overdueDays * dailyRate * quantity * LateFeeMultiplier (1.5).
    /// </summary>
    /// <param name="actualReturnDate">The date when equipment was physically returned.</param>
    /// <param name="agreedEndDate">The original scheduled return date.</param>
    /// <param name="dailyRate">The daily rental rate per unit.</param>
    /// <param name="quantity">The number of units rented.</param>
    /// <returns>The calculated late penalty, or 0 if returned on or before agreed date.</returns>
    /// <exception cref="DomainException">Thrown if rate or quantity are non-positive.</exception>
    public static decimal CalculateLateFee(DateTime actualReturnDate, DateTime agreedEndDate, decimal dailyRate, int quantity)
    {
        if (dailyRate <= 0)
        {
            throw new DomainException("Daily rate must be greater than zero.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        var actualDate = actualReturnDate.Date;
        var agreedDate = agreedEndDate.Date;

        if (actualDate > agreedDate)
        {
            var overdueDays = (actualDate - agreedDate).Days;
            return overdueDays * dailyRate * quantity * LateFeeMultiplier;
        }

        return 0m;
    }

    /// <summary>
    /// Returns a new <see cref="RentalCost"/> instance with the specified late fee applied.
    /// </summary>
    public RentalCost WithLateFee(decimal lateFee)
    {
        if (lateFee < 0)
        {
            throw new DomainException("Late fee cannot be negative.");
        }

        return new RentalCost(Total, lateFee);
    }
}
