using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventManagement.Web.Models;

namespace EventManagement.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Organization)
                    .HasMaxLength(200);

                entity.Property(e => e.Department)
                    .HasMaxLength(50);

                entity.Property(e => e.PreferredLanguage)
                    .HasMaxLength(10)
                    .HasDefaultValue("en-US");

                entity.Property(e => e.TimeZone)
                    .HasMaxLength(50)
                    .HasDefaultValue("UTC");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Indexes for better query performance
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => new { e.FirstName, e.LastName });
                entity.HasIndex(e => e.Organization);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
            });

            // Customize Identity table names if needed
            // builder.Entity<ApplicationUser>().ToTable("Users");
            // builder.Entity<IdentityRole>().ToTable("Roles");
            // builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            // builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            // builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            // builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            // builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}