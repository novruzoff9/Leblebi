using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leblebi.Migrations
{
    /// <inheritdoc />
    public partial class updatedEmpHireDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "HireDate",
                table: "Employees",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2025, 2, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Employees");
        }
    }
}
