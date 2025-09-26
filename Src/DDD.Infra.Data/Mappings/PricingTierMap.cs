using DDD.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infra.Data.Mappings;

public class PricingTierMap : IEntityTypeConfiguration<PricingTier>
{
    public void Configure(EntityTypeBuilder<PricingTier> builder)
    {
        builder.Property(p => p.Id)
            .HasColumnName("Id");

        builder.Property(p => p.EventId)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnType("varchar(3)")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Capacity)
            .IsRequired();

        builder.Property(p => p.AvailableCapacity)
            .IsRequired();

        builder.Property(p => p.SaleStartDate)
            .IsRequired();

        builder.Property(p => p.SaleEndDate)
            .IsRequired();

        // Foreign key relationship
        builder.HasOne<Event>()
            .WithMany(e => e.PricingTiers)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}