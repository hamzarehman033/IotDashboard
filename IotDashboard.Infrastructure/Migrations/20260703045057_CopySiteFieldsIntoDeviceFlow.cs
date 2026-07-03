using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CopySiteFieldsIntoDeviceFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"Address\" text NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"Coordinates\" character varying(100) NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"RegionId\" bigint NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"SubRegionId\" bigint NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"ZoneId\" bigint NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF to_regclass('public.""Sites""') IS NOT NULL THEN
                        UPDATE ""Devices"" d
                        SET
                            ""CustomerId"" = s.""CustomerId"",
                            ""Name"" = s.""Name"",
                            ""Code"" = s.""Code"",
                            ""Status"" = s.""Status"",
                            ""RegionId"" = s.""RegionId"",
                            ""SubRegionId"" = s.""SubRegionId"",
                            ""ZoneId"" = s.""ZoneId"",
                            ""Address"" = s.""Address"",
                            ""Coordinates"" = s.""Coordinates""
                        FROM ""Sites"" s
                        WHERE d.""SiteId"" = s.""Id"";
                    END IF;
                END $$;
            ");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CreatedOn",
                value: new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Devices_RegionId\" ON \"Devices\" (\"RegionId\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Devices_SubRegionId\" ON \"Devices\" (\"SubRegionId\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Devices_ZoneId\" ON \"Devices\" (\"ZoneId\");");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Devices_Locations_RegionId') THEN
                        ALTER TABLE ""Devices""
                            ADD CONSTRAINT ""FK_Devices_Locations_RegionId""
                            FOREIGN KEY (""RegionId"") REFERENCES ""Locations"" (""Id"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Devices_Locations_SubRegionId') THEN
                        ALTER TABLE ""Devices""
                            ADD CONSTRAINT ""FK_Devices_Locations_SubRegionId""
                            FOREIGN KEY (""SubRegionId"") REFERENCES ""Locations"" (""Id"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Devices_Locations_ZoneId') THEN
                        ALTER TABLE ""Devices""
                            ADD CONSTRAINT ""FK_Devices_Locations_ZoneId""
                            FOREIGN KEY (""ZoneId"") REFERENCES ""Locations"" (""Id"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Locations_RegionId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Locations_SubRegionId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Locations_ZoneId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_RegionId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_SubRegionId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_ZoneId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SubRegionId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Devices");

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
    }
}
