using System;

using DDD.Infra.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DDD.Infra.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
public partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.5")
            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

        modelBuilder.Entity("DDD.Domain.Models.Customer", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            b.Property<DateTime>("BirthDate");

            b.Property<string>("Email")
                .IsRequired()
                .HasColumnType("varchar(100)")
                .HasMaxLength(11);

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            b.Property<DateTime>("CreatedAt");

            b.Property<int>("CreatedBy");

            b.Property<DateTime?>("UpdatedAt");

            b.Property<int?>("UpdatedBy");

            b.Property<bool>("IsDeleted");

            b.HasKey("Id");

            b.ToTable("Customers");
        });

        modelBuilder.Entity("DDD.Domain.Models.Event", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            b.Property<string>("Title")
                .IsRequired()
                .HasColumnType("varchar(200)")
                .HasMaxLength(200);

            b.Property<string>("Description")
                .IsRequired()
                .HasColumnType("text");

            b.Property<Guid>("OrganizerId")
                .IsRequired();

            b.Property<Guid>("VenueId")
                .IsRequired();

            b.Property<DateTime>("StartDate")
                .IsRequired();

            b.Property<DateTime>("EndDate")
                .IsRequired();

            b.Property<string>("Status")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property<string>("Visibility")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property<int?>("TotalCapacity");

            b.Property<DateTime>("CreatedAt");

            b.Property<int>("CreatedBy");

            b.Property<DateTime?>("UpdatedAt");

            b.Property<int?>("UpdatedBy");

            b.Property<bool>("IsDeleted");

            b.HasKey("Id");

            b.ToTable("Events");
        });

        modelBuilder.Entity("DDD.Domain.Models.Venue", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("varchar(200)")
                .HasMaxLength(200);

            b.Property<string>("Address")
                .IsRequired()
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);

            b.Property<int>("Capacity")
                .IsRequired();

            b.Property<DateTime>("CreatedAt");

            b.Property<int>("CreatedBy");

            b.Property<DateTime?>("UpdatedAt");

            b.Property<int?>("UpdatedBy");

            b.Property<bool>("IsDeleted");

            b.HasKey("Id");

            b.ToTable("Venues");
        });

        modelBuilder.Entity("DDD.Domain.Models.PricingTier", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            b.Property<Guid>("EventId")
                .IsRequired();

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            b.Property<decimal>("Price")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property<string>("Currency")
                .IsRequired()
                .HasColumnType("varchar(3)")
                .HasMaxLength(3);

            b.Property<int>("Capacity")
                .IsRequired();

            b.Property<int>("AvailableCapacity")
                .IsRequired();

            b.Property<DateTime>("SaleStartDate")
                .IsRequired();

            b.Property<DateTime>("SaleEndDate")
                .IsRequired();

            b.Property<DateTime>("CreatedAt");

            b.Property<int>("CreatedBy");

            b.Property<DateTime?>("UpdatedAt");

            b.Property<int?>("UpdatedBy");

            b.Property<bool>("IsDeleted");

            b.HasKey("Id");

            b.HasIndex("EventId");

            b.ToTable("PricingTiers");
        });

        modelBuilder.Entity("DDD.Domain.Models.PricingTier", b =>
        {
            b.HasOne("DDD.Domain.Models.Event")
                .WithMany("PricingTiers")
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
