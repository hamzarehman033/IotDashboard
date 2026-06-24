using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelemetryStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceTelemetryLatest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SiteId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SummaryPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsCrcValid = table.Column<bool>(type: "boolean", nullable: true),
                    DecodeError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTelemetryLatest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SiteId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecodedPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsCrcValid = table.Column<bool>(type: "boolean", nullable: true),
                    DecodeError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryMessages", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2447));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2449));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2451));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2453));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2455));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2457));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 24, 12, 42, 15, 350, DateTimeKind.Utc).AddTicks(2459));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetryLatest_DeviceId",
                table: "DeviceTelemetryLatest",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetryLatest_SiteId",
                table: "DeviceTelemetryLatest",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryMessages_ReceivedAtUtc",
                table: "TelemetryMessages",
                column: "ReceivedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryMessages_TenantId_SiteId_DeviceId_ReceivedAtUtc",
                table: "TelemetryMessages",
                columns: new[] { "TenantId", "SiteId", "DeviceId", "ReceivedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTelemetryLatest");

            migrationBuilder.DropTable(
                name: "TelemetryMessages");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5617));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5621));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5623));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5625));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5627));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5629));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5630));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5632));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 21, 19, 36, 25, 320, DateTimeKind.Utc).AddTicks(5634));
        }
    }
}
