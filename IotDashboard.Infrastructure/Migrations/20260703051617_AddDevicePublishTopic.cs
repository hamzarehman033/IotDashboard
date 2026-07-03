using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePublishTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Devices\" ADD COLUMN IF NOT EXISTS \"PublishTopic\" character varying(255) NOT NULL DEFAULT ''; ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Devices\" DROP COLUMN IF EXISTS \"PublishTopic\";");
        }
    }
}
