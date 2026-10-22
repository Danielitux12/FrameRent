using FrameRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameRent.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration mapping the Equipment domain aggregate root to relational schema.
/// </summary>
public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.DailyRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.TotalUnits)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);

        // One-to-many relationship with Rental contracts. DeleteBehavior.Restrict prevents cascading delete.
        builder.HasMany(e => e.Rentals)
            .WithOne(r => r.Equipment)
            .HasForeignKey(r => r.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
