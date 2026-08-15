using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiVisionPackets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiVisionPackets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceNumber = table.Column<int>(type: "integer", nullable: false),
                    Topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PacketSignature = table.Column<int>(type: "integer", nullable: false),
                    ProtocolVersion = table.Column<byte>(type: "smallint", nullable: false),
                    MessageType = table.Column<byte>(type: "smallint", nullable: false),
                    HeaderLength = table.Column<int>(type: "integer", nullable: false),
                    Flags = table.Column<int>(type: "integer", nullable: false),
                    PacketSequence = table.Column<long>(type: "bigint", nullable: false),
                    TimestampUtc = table.Column<long>(type: "bigint", nullable: false),
                    SiteIdHash = table.Column<long>(type: "bigint", nullable: false),
                    EdgeDeviceIdHash = table.Column<long>(type: "bigint", nullable: false),
                    MessageIdHash = table.Column<long>(type: "bigint", nullable: false),
                    EventIdHash = table.Column<long>(type: "bigint", nullable: false),
                    CameraId = table.Column<byte>(type: "smallint", nullable: false),
                    EventType = table.Column<byte>(type: "smallint", nullable: false),
                    Severity = table.Column<byte>(type: "smallint", nullable: false),
                    ConfidenceRaw = table.Column<int>(type: "integer", nullable: false),
                    ActivityZone = table.Column<byte>(type: "smallint", nullable: false),
                    ObjectCount = table.Column<int>(type: "integer", nullable: false),
                    EhsCodeCount = table.Column<byte>(type: "smallint", nullable: false),
                    EhsCodes = table.Column<byte[]>(type: "bytea", nullable: false),
                    SnapshotReasonCode = table.Column<byte>(type: "smallint", nullable: false),
                    ActiveCameraCount = table.Column<byte>(type: "smallint", nullable: false),
                    ConfiguredCameraCount = table.Column<byte>(type: "smallint", nullable: false),
                    DetectionEnabled = table.Column<byte>(type: "smallint", nullable: false),
                    SystemStatus = table.Column<byte>(type: "smallint", nullable: false),
                    HeartbeatIntervalSec = table.Column<int>(type: "integer", nullable: false),
                    EdgeUptimeSec = table.Column<long>(type: "bigint", nullable: false),
                    CpuUsagePercent = table.Column<byte>(type: "smallint", nullable: false),
                    RamUsagePercent = table.Column<byte>(type: "smallint", nullable: false),
                    DiskFreePercent = table.Column<byte>(type: "smallint", nullable: false),
                    CameraStatusBitmap = table.Column<int>(type: "integer", nullable: false),
                    ModelId = table.Column<byte>(type: "smallint", nullable: false),
                    ImageFormat = table.Column<byte>(type: "smallint", nullable: false),
                    ImageEncoding = table.Column<byte>(type: "smallint", nullable: false),
                    ImageWidth = table.Column<int>(type: "integer", nullable: false),
                    ImageHeight = table.Column<int>(type: "integer", nullable: false),
                    ImageSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ImageCrc32 = table.Column<long>(type: "bigint", nullable: false),
                    HeaderCrc16 = table.Column<int>(type: "integer", nullable: false),
                    IsHeaderCrcValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsImageCrcValid = table.Column<bool>(type: "boolean", nullable: false),
                    ImageBytes = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiVisionPackets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiVisionPackets_DeviceNumber_MessageIdHash_PacketSequence",
                table: "AiVisionPackets",
                columns: new[] { "DeviceNumber", "MessageIdHash", "PacketSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiVisionPackets_DeviceNumber_MessageType_ReceivedAtUtc",
                table: "AiVisionPackets",
                columns: new[] { "DeviceNumber", "MessageType", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AiVisionPackets_DeviceNumber_ReceivedAtUtc",
                table: "AiVisionPackets",
                columns: new[] { "DeviceNumber", "ReceivedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiVisionPackets");
        }
    }
}
