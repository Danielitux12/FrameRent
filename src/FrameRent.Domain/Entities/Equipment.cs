using FrameRent.Domain.Exceptions;

namespace FrameRent.Domain.Entities;

/// <summary>
/// Represents a rentable equipment item in the inventory (e.g. scaffolding, props, frames).
/// Serves as the Aggregate Root for equipment inventory management.
/// </summary>
public class Equipment
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal DailyRate { get; private set; }
    public int TotalUnits { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property for associated rental contracts.
    /// No business logic should be executed directly against this collection inside the Equipment aggregate.
    /// </summary>
    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();

    // Parameterless constructor required by EF Core
    protected Equipment() { }

    private Equipment(Guid id, string name, string category, string description, decimal dailyRate, int totalUnits, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Category = category;
        Description = description;
        DailyRate = dailyRate;
        TotalUnits = totalUnits;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    /// <summary>
    /// Factory method to instantiate new equipment with validated business constraints.
    /// </summary>
    /// <param name="name">Name of the equipment.</param>
    /// <param name="category">Category grouping (e.g. Scaffolding, Safety, Machinery).</param>
    /// <param name="description">Detailed description of technical specs.</param>
    /// <param name="dailyRate">Daily rental price per unit.</param>
    /// <param name="totalUnits">Total stock units owned and available in inventory.</param>
    /// <returns>A new <see cref="Equipment"/> aggregate instance.</returns>
    /// <exception cref="DomainException">Thrown if validation rules are violated.</exception>
    public static Equipment Create(string name, string category, string description, decimal dailyRate, int totalUnits)
    {
        ValidateEquipmentData(name, category, dailyRate, totalUnits);

        return new Equipment(
            Guid.NewGuid(),
            name.Trim(),
            category.Trim(),
            description?.Trim() ?? string.Empty,
            dailyRate,
            totalUnits,
            DateTime.UtcNow
        );
    }

    /// <summary>
    /// Updates the equipment details and records modification timestamp.
    /// </summary>
    public void Update(string name, string category, string description, decimal dailyRate, int totalUnits)
    {
        ValidateEquipmentData(name, category, dailyRate, totalUnits);

        Name = name.Trim();
        Category = category.Trim();
        Description = description?.Trim() ?? string.Empty;
        DailyRate = dailyRate;
        TotalUnits = totalUnits;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the equipment so it can no longer be rented.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Checking whether this equipment currently has active, approved, or delivered rentals
    /// is not validated here directly because it crosses the Rental aggregate boundary.
    /// That coordination rule is enforced by the Application layer via repository queries before invoking Deactivate.
    /// </remarks>
    /// <exception cref="DomainException">Thrown if the equipment is already inactive.</exception>
    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("Equipment is already inactive.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateEquipmentData(string name, string category, decimal dailyRate, int totalUnits)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Equipment name cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Equipment category cannot be empty or whitespace.");
        }

        if (dailyRate <= 0)
        {
            throw new DomainException("Daily rate must be strictly greater than zero.");
        }

        if (totalUnits <= 0)
        {
            throw new DomainException("Total units must be strictly greater than zero.");
        }
    }
}
