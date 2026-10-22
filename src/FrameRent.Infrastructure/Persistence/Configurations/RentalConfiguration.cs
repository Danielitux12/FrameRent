using FrameRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameRent.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration mapping the Rental aggregate root and its owned value objects (Period, Cost) to relational schema.
/// </summary>
public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("Rentals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ClientId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.Quantity)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.ApprovedAt);
        builder.Property(r => r.DeliveredAt);
        builder.Property(r => r.ReturnedAt);
        builder.Property(r => r.CancelledAt);
        builder.Property(r => r.RejectedAt);

        // Map DateRange value object as an owned type with explicit column names
        builder.OwnsOne(r => r.Period, periodBuilder =>
        {
            periodBuilder.Property(p => p.Start)
                .HasColumnName("PeriodStart")
                .IsRequired();

            periodBuilder.Property(p => p.End)
                .HasColumnName("PeriodEnd")
                .IsRequired();
        });

        // Map RentalCost value object as an owned type with explicit column names and precision
        builder.OwnsOne(r => r.Cost, costBuilder =>
        {
            costBuilder.Property(c => c.Total)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("TotalCost")
                .IsRequired();

            costBuilder.Property(c => c.LateFee)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("LateFee");
        });

        builder.HasIndex(r => r.ClientId);
        builder.HasIndex(r => r.EquipmentId);
        builder.HasIndex(r => r.Status);
    }
}
