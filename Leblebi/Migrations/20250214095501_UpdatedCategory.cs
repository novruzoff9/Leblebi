using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leblebi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_ExpenseCategories_ParentCategoryId",
                table: "ExpenseCategories");

            migrationBuilder.AlterColumn<int>(
                name: "ParentCategoryId",
                table: "ExpenseCategories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_ExpenseCategories_ParentCategoryId",
                table: "ExpenseCategories",
                column: "ParentCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_ExpenseCategories_ParentCategoryId",
                table: "ExpenseCategories");

            migrationBuilder.AlterColumn<int>(
                name: "ParentCategoryId",
                table: "ExpenseCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_ExpenseCategories_ParentCategoryId",
                table: "ExpenseCategories",
                column: "ParentCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
