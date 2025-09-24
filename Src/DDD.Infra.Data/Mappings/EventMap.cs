using DDD.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infra.Data.Mappings;

public class EventMap : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Id)
            .HasColumnName("Id");

        builder.Property(e => e.Title)
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.OrganizerId)
            .IsRequired();

        builder.Property(e => e.VenueId)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Visibility)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Add query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}