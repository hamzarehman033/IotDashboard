using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSiteIdWithDeviceIdLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Devices_Sites_SiteId') THEN
                        ALTER TABLE ""Devices"" DROP CONSTRAINT ""FK_Devices_Sites_SiteId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Sites\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_TelemetryMessages_TenantId_SiteId_DeviceId_ReceivedAtUtc\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_DeviceTelemetryLatest_SiteId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Devices_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"TelemetryMessages\" DROP COLUMN IF EXISTS \"SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"TelecomTelemetryPackets\" DROP COLUMN IF EXISTS \"SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"TelecomTelemetryPackets\" DROP COLUMN IF EXISTS \"SiteIdHash\";");
            migrationBuilder.Sql("ALTER TABLE \"DeviceTelemetryLatest\" DROP COLUMN IF EXISTS \"SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Devices\" DROP COLUMN IF EXISTS \"SiteId\";");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_TelemetryMessages_TenantId_DeviceId_ReceivedAtUtc\" ON \"TelemetryMessages\" (\"TenantId\", \"DeviceId\", \"ReceivedAtUtc\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryMessages_TenantId_DeviceId_ReceivedAtUtc",
                table: "TelemetryMessages");

            migrationBuilder.AddColumn<string>(
                name: "SiteId",
                table: "TelemetryMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SiteId",
                table: "TelecomTelemetryPackets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SiteIdHash",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteId",
                table: "DeviceTelemetryLatest",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SiteId",
                table: "Devices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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
                    Address = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Coordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryMessages_TenantId_SiteId_DeviceId_ReceivedAtUtc",
                table: "TelemetryMessages",
                columns: new[] { "TenantId", "SiteId", "DeviceId", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetryLatest_SiteId",
                table: "DeviceTelemetryLatest",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SiteId",
                table: "Devices",
                column: "SiteId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Sites_SiteId",
                table: "Devices",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
