using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leblebi.Migrations
{
    /// <inheritdoc />
    public partial class addedEmpFire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FireDate",
                table: "Employees",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FireDate",
                table: "Employees");
        }
    }
}
