using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    [DbContext(typeof(AppDBContext))]
    [Migration("20260901122100_AddActivityDeviceId")]
    public partial class AddActivityDeviceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeviceId",
                table: "Activities",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DeviceId_Date",
                table: "Activities",
                columns: new[] { "DeviceId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Devices_DeviceId",
                table: "Activities",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Devices_DeviceId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_DeviceId_Date",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Activities");
        }
    }
}
