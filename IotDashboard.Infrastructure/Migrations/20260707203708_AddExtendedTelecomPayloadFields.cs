using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedTelecomPayloadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GensetPowerW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tenant1CurrentA",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tenant1LoadW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tenant2CurrentA",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tenant2LoadW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tenant3CurrentA",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tenant3LoadW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tenant4CurrentA",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tenant4LoadW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GensetPowerW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant1CurrentA",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant1LoadW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant2CurrentA",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant2LoadW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant3CurrentA",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant3LoadW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant4CurrentA",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Tenant4LoadW",
                table: "TelecomTelemetryPackets");
        }
    }
}
