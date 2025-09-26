using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DDD.Infra.Data.Migrations;

public partial class AddEventsAndPricingTiers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Venues table
        migrationBuilder.CreateTable(
            name: "Venues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Venues", x => x.Id);
            });

        // Create Events table
        migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                OrganizerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                TotalCapacity = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Events", x => x.Id);
                table.ForeignKey(
                    name: "FK_Events_Venues_VenueId",
                    column: x => x.VenueId,
                    principalTable: "Venues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Create PricingTiers table
        migrationBuilder.CreateTable(
            name: "PricingTiers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                AvailableCapacity = table.Column<int>(type: "int", nullable: false),
                SaleStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                SaleEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PricingTiers", x => x.Id);
                table.ForeignKey(
                    name: "FK_PricingTiers_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Create indexes
        migrationBuilder.CreateIndex(
            name: "IX_Events_VenueId",
            table: "Events",
            column: "VenueId");

        migrationBuilder.CreateIndex(
            name: "IX_PricingTiers_EventId",
            table: "PricingTiers",
            column: "EventId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PricingTiers");

        migrationBuilder.DropTable(
            name: "Events");

        migrationBuilder.DropTable(
            name: "Venues");
    }
}