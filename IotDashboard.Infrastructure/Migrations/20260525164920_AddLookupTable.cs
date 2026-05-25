using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "Category", "CreatedBy", "CreatedOn", "Description", "IsActive", "ModifiedBy", "ModifiedOn", "Name", "Order", "Value" },
                values: new object[,]
                {
                    { 1L, "LocationType", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5451), null, true, null, null, "Region", 1, "1" },
                    { 2L, "LocationType", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5457), null, true, null, null, "SubRegion", 2, "2" },
                    { 3L, "LocationType", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5461), null, true, null, null, "Zone", 3, "3" },
                    { 4L, "Status", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5465), null, true, null, null, "Active", 1, "Active" },
                    { 5L, "Status", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5469), null, true, null, null, "Inactive", 2, "Inactive" },
                    { 6L, "Status", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5473), null, true, null, null, "Suspended", 3, "Suspended" },
                    { 7L, "SubscriptionStatus", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5476), null, true, null, null, "Active", 1, "Active" },
                    { 8L, "SubscriptionStatus", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5480), null, true, null, null, "Inactive", 2, "Inactive" },
                    { 9L, "SubscriptionStatus", 1L, new DateTime(2026, 5, 25, 16, 49, 20, 319, DateTimeKind.Utc).AddTicks(5483), null, true, null, null, "Expired", 3, "Expired" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_Category",
                table: "Lookups",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lookups");
        }
    }
}
