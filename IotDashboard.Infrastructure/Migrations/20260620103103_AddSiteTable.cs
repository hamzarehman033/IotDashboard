using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false),
                    SubRegionId = table.Column<long>(type: "bigint", nullable: false),
                    ZoneId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Coordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sites_Locations_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sites_Locations_SubRegionId",
                        column: x => x.SubRegionId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sites_Locations_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1313));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1317));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1319));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1321));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1322));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1324));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1325));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1326));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 10, 31, 3, 446, DateTimeKind.Utc).AddTicks(1328));

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CustomerId_Code",
                table: "Sites",
                columns: new[] { "CustomerId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_RegionId",
                table: "Sites",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_SubRegionId",
                table: "Sites",
                column: "SubRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_ZoneId",
                table: "Sites",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(886));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(891));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(893));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(935));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(942));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(943));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 21, 33, 53, 660, DateTimeKind.Utc).AddTicks(945));
        }
    }
}
