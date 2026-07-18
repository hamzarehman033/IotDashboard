using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelecomTelemetryPacketV12Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "ActivePowerSource",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Alarm4Code",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Alarm4Level",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Alarm5Code",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Alarm5Level",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Alarm6Code",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Alarm6Level",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "BatteryContactorStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryCycleCount",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "BatteryLvdStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BatteryMaxCellTemp",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryMaxCellVoltageMv",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryMinCellVoltageMv",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "BatterySoc",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryStatusExtended",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BatteryTotalDischargeEnergyWh",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryTotalDischargeTimes",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DataValidityBitmap",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DcBranchAlarmBitmap",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DcCriticalLoadCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DcFuseAlarmBitmap",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DcLvd1Status",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DcLvd2Status",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DcNonCriticalLoadCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeviceUptimeSeconds",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetFrequency",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL1Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL1Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL2Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL2Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL3Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtGensetL3Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExtGensetTotalEnergyWh",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExtGensetTotalPowerW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainFrequency",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL1Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL1Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL2Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL2Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL3Current",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtMainL3Voltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExtMainTotalEnergyWh",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExtMainTotalPowerW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionCrc16",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelConsumptionRateLph",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FuelRuntimeRemainingMin",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FuelSensorStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FuelTankCapacityL",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "GatewayCpuUsagePercent",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "GatewayRamUsagePercent",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GatewayTemperature",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetBatteryVoltage",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetCurrentL1",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetCurrentL2",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetCurrentL3",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetFrequency",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetFuelConsumptionRateLph",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GensetNextServiceHours",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetVoltageL1",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetVoltageL2",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetVoltageL3",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HybridModeEnabled",
                table: "TelecomTelemetryPackets",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtensionCrcValid",
                table: "TelecomTelemetryPackets",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSuccessfulPollAgeSeconds",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "NetworkType",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PowerSourcePriority",
                table: "TelecomTelemetryPackets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rectifier1OutputCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rectifier2OutputCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rectifier3OutputCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rectifier4OutputCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RectifierCapacityTotalW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RectifierCapacityUsedPercent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RectifierEfficiencyPercent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RectifierFaultCount",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RectifierHighestLoadModulePercent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RectifierRedundancyStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "SignalStrengthDbm",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SimStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

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

            migrationBuilder.AddColumn<decimal>(
                name: "SolarBatteryChargeCurrent",
                table: "TelecomTelemetryPackets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SolarControllerFaultCount",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SolarDailyPeakPowerW",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SolarMpptStatus",
                table: "TelecomTelemetryPackets",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SolarPanelStringAlarmBitmap",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SolarTotalEnergyLifetimeWh",
                table: "TelecomTelemetryPackets",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivePowerSource",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm4Code",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm4Level",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm5Code",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm5Level",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm6Code",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Alarm6Level",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryContactorStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryCycleCount",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryLvdStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryMaxCellTemp",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryMaxCellVoltageMv",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryMinCellVoltageMv",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatterySoc",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryStatusExtended",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryTotalDischargeEnergyWh",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "BatteryTotalDischargeTimes",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DataValidityBitmap",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcBranchAlarmBitmap",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcCriticalLoadCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcFuseAlarmBitmap",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcLvd1Status",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcLvd2Status",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DcNonCriticalLoadCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "DeviceUptimeSeconds",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetFrequency",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL1Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL1Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL2Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL2Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL3Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetL3Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetTotalEnergyWh",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtGensetTotalPowerW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainFrequency",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL1Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL1Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL2Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL2Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL3Current",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainL3Voltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainTotalEnergyWh",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtMainTotalPowerW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "ExtensionCrc16",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "FuelConsumptionRateLph",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "FuelRuntimeRemainingMin",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "FuelSensorStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "FuelTankCapacityL",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GatewayCpuUsagePercent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GatewayRamUsagePercent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GatewayTemperature",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetBatteryVoltage",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetCurrentL1",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetCurrentL2",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetCurrentL3",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetFrequency",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetFuelConsumptionRateLph",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetNextServiceHours",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetVoltageL1",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetVoltageL2",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "GensetVoltageL3",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "HybridModeEnabled",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "IsExtensionCrcValid",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulPollAgeSeconds",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "NetworkType",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "PowerSourcePriority",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Rectifier1OutputCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Rectifier2OutputCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Rectifier3OutputCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "Rectifier4OutputCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierCapacityTotalW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierCapacityUsedPercent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierEfficiencyPercent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierFaultCount",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierHighestLoadModulePercent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "RectifierRedundancyStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SignalStrengthDbm",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SimStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SiteIdHash",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarBatteryChargeCurrent",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarControllerFaultCount",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarDailyPeakPowerW",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarMpptStatus",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarPanelStringAlarmBitmap",
                table: "TelecomTelemetryPackets");

            migrationBuilder.DropColumn(
                name: "SolarTotalEnergyLifetimeWh",
                table: "TelecomTelemetryPackets");
        }
    }
}
