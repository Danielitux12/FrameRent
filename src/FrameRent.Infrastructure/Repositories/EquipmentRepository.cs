using FrameRent.Domain.Entities;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

/// <summary>
/// Relational database repository implementation for the Equipment domain aggregate.
/// </summary>
public class EquipmentRepository : IEquipmentRepository
{
    private readonly FrameRentDbContext _context;

    public EquipmentRepository(FrameRentDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<Equipment>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.Equipment
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Equipment>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Equipment
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken ct = default)
    {
        await _context.Equipment.AddAsync(equipment, ct);
    }

    public void Update(Equipment equipment)
    {
        _context.Equipment.Update(equipment);
    }
}
