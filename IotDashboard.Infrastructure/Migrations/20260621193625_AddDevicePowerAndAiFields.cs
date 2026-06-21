using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePowerAndAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AiEhsInstalled",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AiSecurityInstalled",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BatteryBrand",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BatteryCapacity",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BatteryQty",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CamerasInstalledCount",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GeneratorBrand",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GeneratorCapacity",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GeneratorQty",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RectifierBrand",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RectifierCapacity",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RectifierQty",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RmsSerialNumber",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SimCardNumber",
                table: "Devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SolarBrand",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SolarCapacity",
                table: "Devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SolarQty",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiEhsInstalled",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "AiSecurityInstalled",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BatteryBrand",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BatteryCapacity",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BatteryQty",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CamerasInstalledCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GeneratorBrand",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GeneratorCapacity",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GeneratorQty",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RectifierBrand",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RectifierCapacity",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RectifierQty",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RmsSerialNumber",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SimCardNumber",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SolarBrand",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SolarCapacity",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SolarQty",
                table: "Devices");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1726));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1730));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1732));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1734));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1772));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1774));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1776));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1777));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 20, 19, 37, 26, 11, DateTimeKind.Utc).AddTicks(1779));
        }
    }
}
