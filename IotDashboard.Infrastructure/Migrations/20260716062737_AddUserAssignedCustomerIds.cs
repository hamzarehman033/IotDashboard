using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAssignedCustomerIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<long>>(
                name: "AssignedCustomerIds",
                table: "User",
                type: "bigint[]",
                nullable: false,
                defaultValueSql: "'{}'::bigint[]");

            migrationBuilder.Sql(
                """
                UPDATE "User"
                SET "AssignedCustomerIds" = ARRAY["CustomerId"]::bigint[]
                WHERE "CustomerId" IS NOT NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedCustomerIds",
                table: "User");
        }
    }
}
