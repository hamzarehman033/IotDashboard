using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TelecomTelemetryPacketAddedLocationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RegionId",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubRegionId",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ZoneId",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4780));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4783));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4785));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4788));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4789));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4791));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4792));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 29, 20, 47, 19, 291, DateTimeKind.Utc).AddTicks(4794));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SubRegionId",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "TelecomTelemetryPackets");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6357));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6361));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6363));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6364));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6368));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6369));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6371));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6372));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 27, 22, 28, 50, 187, DateTimeKind.Utc).AddTicks(6373));
        }
    }
}
