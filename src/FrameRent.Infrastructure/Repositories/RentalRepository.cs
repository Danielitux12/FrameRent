using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

/// <summary>
/// Relational database repository implementation for the Rental domain aggregate.
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly FrameRentDbContext _context;

    public RentalRepository(FrameRentDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Rentals
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Rental?> GetByIdWithEquipmentAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Rentals
            .Include(r => r.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Rental>> GetByClientIdAsync(string clientId, CancellationToken ct = default)
    {
        return await _context.Rentals
            .Include(r => r.Equipment)
            .Where(r => r.ClientId == clientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Rental>> GetAllAsync(RentalStatus? status, CancellationToken ct = default)
    {
        var query = _context.Rentals
            .Include(r => r.Equipment)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Rental>> GetBlockingRentalsForEquipmentAsync(Guid equipmentId, CancellationToken ct = default)
    {
        return await _context.Rentals
            .Where(r => r.EquipmentId == equipmentId &&
                        (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered))
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveCommitmentsAsync(Guid equipmentId, CancellationToken ct = default)
    {
        return await _context.Rentals
            .AnyAsync(r => r.EquipmentId == equipmentId &&
                           (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered), ct);
    }

    public async Task<int> CountActiveForClientAsync(string clientId, CancellationToken ct = default)
    {
        return await _context.Rentals
            .CountAsync(r => r.ClientId == clientId &&
                             (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Approved), ct);
    }

    public async Task<List<(Guid EquipmentId, int RentalCount)>> GetMostRentedEquipmentAsync(CancellationToken ct = default)
    {
        var grouped = await _context.Rentals
            .GroupBy(r => r.EquipmentId)
            .Select(g => new
            {
                EquipmentId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return grouped.Select(x => (x.EquipmentId, x.Count)).ToList();
    }

    public async Task AddAsync(Rental rental, CancellationToken ct = default)
    {
        await _context.Rentals.AddAsync(rental, ct);
    }

    public void Update(Rental rental)
    {
        _context.Rentals.Update(rental);
    }
}
