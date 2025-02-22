﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leblebi.Migrations
{
    /// <inheritdoc />
    public partial class AddedManagementExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagementCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagementExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManagementCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementExpenses_ManagementCategories_ManagementCategoryId",
                        column: x => x.ManagementCategoryId,
                        principalTable: "ManagementCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementExpenses_ManagementCategoryId",
                table: "ManagementExpenses",
                column: "ManagementCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementExpenses");

            migrationBuilder.DropTable(
                name: "ManagementCategories");
        }
    }
}
