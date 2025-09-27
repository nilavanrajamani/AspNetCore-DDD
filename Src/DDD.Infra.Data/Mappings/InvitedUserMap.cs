using DDD.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infra.Data.Mappings;

public class InvitedUserMap : IEntityTypeConfiguration<InvitedUser>
{
    public void Configure(EntityTypeBuilder<InvitedUser> builder)
    {
        builder.Property(i => i.Id)
            .HasColumnName("Id");

        builder.Property(i => i.EventId)
            .IsRequired();

        builder.Property(i => i.UserId)
            .IsRequired();

        builder.Property(i => i.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Create composite index for performance
        builder.HasIndex(i => new { i.EventId, i.UserId })
            .IsUnique()
            .HasDatabaseName("IX_InvitedUsers_EventId_UserId");

        // Add query filter for soft delete
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}