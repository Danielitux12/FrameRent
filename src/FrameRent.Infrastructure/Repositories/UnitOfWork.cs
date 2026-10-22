using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;

namespace FrameRent.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation wrapping EF Core DbContext transaction coordination.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly FrameRentDbContext _context;

    public UnitOfWork(FrameRentDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
