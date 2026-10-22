using FrameRent.Domain.Exceptions;

namespace FrameRent.Domain.ValueObjects;

/// <summary>
/// Represents an immutable rental date range, tracking inclusive calendar dates without time components.
/// Enforces business constraints: start date not in past, end strictly after start, and duration capped at 15 days.
/// </summary>
public sealed record DateRange
{
    /// <summary>
    /// Maximum allowed duration in calendar days for a single rental reservation.
    /// </summary>
    public const int MaxDurationDays = 15;

    public DateTime Start { get; }
    public DateTime End { get; }

    /// <summary>
    /// Total duration in calendar days.
    /// </summary>
    public int DurationDays => (End - Start).Days;

    private DateRange() { }

    private DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a validated <see cref="DateRange"/> instance.
    /// </summary>
    /// <param name="start">The start date (converted to date-only component).</param>
    /// <param name="end">The end date (converted to date-only component).</param>
    /// <returns>A new immutable <see cref="DateRange"/>.</returns>
    /// <exception cref="DomainException">Thrown if dates are invalid or duration limits are exceeded.</exception>
    public static DateRange Create(DateTime start, DateTime end)
    {
        var startDate = start.Date;
        var endDate = end.Date;
        var today = DateTime.UtcNow.Date;

        if (startDate < today)
        {
            throw new DomainException("Start date cannot be in the past.");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("End date must be strictly after the start date.");
        }

        var duration = (endDate - startDate).Days;
        if (duration > MaxDurationDays)
        {
            throw new DomainException($"Rental duration cannot exceed {MaxDurationDays} days.");
        }

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Determines whether this date range overlaps with another date range.
    /// Two ranges overlap when Start &lt; other.End and End &gt; other.Start.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        if (other is null)
        {
            return false;
        }

        return Start < other.End && End > other.Start;
    }
}
