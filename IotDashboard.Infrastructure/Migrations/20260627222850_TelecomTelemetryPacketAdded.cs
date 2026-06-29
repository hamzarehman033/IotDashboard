using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TelecomTelemetryPacketAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelecomTelemetryPackets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    SiteId = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    TenantNumber = table.Column<int>(type: "integer", nullable: false),
                    SiteNumber = table.Column<int>(type: "integer", nullable: false),
                    DeviceNumber = table.Column<int>(type: "integer", nullable: false),
                    EpochTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PortalReceiveTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PacketVersion = table.Column<byte>(type: "smallint", nullable: true),
                    DeviceType = table.Column<byte>(type: "smallint", nullable: true),
                    Manufacturer = table.Column<byte>(type: "smallint", nullable: true),
                    Model = table.Column<byte>(type: "smallint", nullable: true),
                    SiteIdHash = table.Column<long>(type: "bigint", nullable: true),
                    DeviceIdHash = table.Column<long>(type: "bigint", nullable: true),
                    PacketSequence = table.Column<int>(type: "integer", nullable: true),
                    SystemStatus = table.Column<int>(type: "integer", nullable: true),
                    ActiveAlarmCount = table.Column<byte>(type: "smallint", nullable: true),
                    LineAVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    LineBVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    LineCVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    LineACurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    LineBCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    LineCCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    AcFrequency = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalAcInputPowerW = table.Column<long>(type: "bigint", nullable: true),
                    TotalAcEnergyWh = table.Column<long>(type: "bigint", nullable: true),
                    MainsAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    MainsFailure = table.Column<bool>(type: "boolean", nullable: true),
                    DcBusVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    DcLoadCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    DcLoadPowerW = table.Column<long>(type: "bigint", nullable: true),
                    DcLoadPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalDcEnergyWh = table.Column<long>(type: "bigint", nullable: true),
                    RectifierInstalledCount = table.Column<byte>(type: "smallint", nullable: true),
                    RectifierCommCount = table.Column<byte>(type: "smallint", nullable: true),
                    RectifierTotalCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    RectifierTotalDcPowerW = table.Column<long>(type: "bigint", nullable: true),
                    RectifierAcFail = table.Column<bool>(type: "boolean", nullable: true),
                    RectifierMissing = table.Column<bool>(type: "boolean", nullable: true),
                    RectifierMaxTemperature = table.Column<decimal>(type: "numeric", nullable: true),
                    BatteryStatus = table.Column<byte>(type: "smallint", nullable: true),
                    BatteryVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    BatteryCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    BatteryRemainingPercent = table.Column<byte>(type: "smallint", nullable: true),
                    BatteryTotalCapacityAh = table.Column<decimal>(type: "numeric", nullable: true),
                    BatteryRemainingCapacityAh = table.Column<decimal>(type: "numeric", nullable: true),
                    BatteryBackupTimeMin = table.Column<int>(type: "integer", nullable: true),
                    BatteryTemperature = table.Column<decimal>(type: "numeric", nullable: true),
                    BatterySoh = table.Column<byte>(type: "smallint", nullable: true),
                    BmuOnlineCount = table.Column<byte>(type: "smallint", nullable: true),
                    BatteryChargeDischargeKw = table.Column<decimal>(type: "numeric", nullable: true),
                    SolarAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    SolarVoltage = table.Column<decimal>(type: "numeric", nullable: true),
                    SolarCurrent = table.Column<decimal>(type: "numeric", nullable: true),
                    SolarPowerW = table.Column<long>(type: "bigint", nullable: true),
                    SolarEnergyTodayWh = table.Column<long>(type: "bigint", nullable: true),
                    SolarControllerCount = table.Column<byte>(type: "smallint", nullable: true),
                    SolarControllerCommFail = table.Column<byte>(type: "smallint", nullable: true),
                    SolarChargingHours = table.Column<int>(type: "integer", nullable: true),
                    GensetAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    GensetRunning = table.Column<bool>(type: "boolean", nullable: true),
                    GensetStartFailure = table.Column<bool>(type: "boolean", nullable: true),
                    GensetControlMode = table.Column<byte>(type: "smallint", nullable: true),
                    GensetRunHours = table.Column<int>(type: "integer", nullable: true),
                    GensetStartCount = table.Column<int>(type: "integer", nullable: true),
                    FuelLevelPercent = table.Column<byte>(type: "smallint", nullable: true),
                    FuelVolumeL = table.Column<int>(type: "integer", nullable: true),
                    FuelTheftAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    FuelLowAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    AmbientTemperature1 = table.Column<decimal>(type: "numeric", nullable: true),
                    AmbientTemperature2 = table.Column<decimal>(type: "numeric", nullable: true),
                    Humidity = table.Column<decimal>(type: "numeric", nullable: true),
                    DoorOpenAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    SmokeAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    WaterLeakAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    MotionAlarm = table.Column<bool>(type: "boolean", nullable: true),
                    DigitalInputBitmap = table.Column<int>(type: "integer", nullable: true),
                    RelayOutputBitmap = table.Column<int>(type: "integer", nullable: true),
                    Alarm1Code = table.Column<int>(type: "integer", nullable: true),
                    Alarm1Level = table.Column<byte>(type: "smallint", nullable: true),
                    Alarm2Code = table.Column<int>(type: "integer", nullable: true),
                    Alarm2Level = table.Column<byte>(type: "smallint", nullable: true),
                    Alarm3Code = table.Column<int>(type: "integer", nullable: true),
                    Alarm3Level = table.Column<byte>(type: "smallint", nullable: true),
                    AlarmBitmap1 = table.Column<long>(type: "bigint", nullable: true),
                    Crc16 = table.Column<int>(type: "integer", nullable: true),
                    IsCrcValid = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelecomTelemetryPackets", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelecomTelemetryPackets");

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
        }
    }
}
