using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserModulesBigintArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<long>>(
                name: "Modules",
                table: "User",
                type: "bigint[]",
                nullable: false,
                defaultValueSql: "'{}'::bigint[]");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Modules",
                table: "User");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8620));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8625));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8627));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8629));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8632));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8634));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8635));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 18, 20, 44, 34, 524, DateTimeKind.Utc).AddTicks(8637));
        }
    }
}
