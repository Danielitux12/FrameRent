using FrameRent.Domain.Entities;
using FrameRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context integrating ASP.NET Core Identity tables
/// with FrameRent domain aggregates (Equipment, Rental).
/// </summary>
public class FrameRentDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Rental> Rentals => Set<Rental>();

    public FrameRentDbContext(DbContextOptions<FrameRentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(FrameRentDbContext).Assembly);
    }
}
