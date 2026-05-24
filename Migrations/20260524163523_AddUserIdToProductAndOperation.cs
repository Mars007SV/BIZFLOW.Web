using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZFLOW.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToProductAndOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add UserId column to Products table with default value 0 (temporary)
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Add UserId column to Operations table with default value 0 (temporary)
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Operations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Update existing Products to assign to first user (if exists)
            migrationBuilder.Sql(@"
                UPDATE Products 
                SET UserId = (SELECT Id FROM Users ORDER BY Id LIMIT 1)
                WHERE UserId = 0 AND EXISTS (SELECT 1 FROM Users);
            ");

            // Update existing Operations to assign to first user (if exists)
            migrationBuilder.Sql(@"
                UPDATE Operations 
                SET UserId = (SELECT Id FROM Users ORDER BY Id LIMIT 1)
                WHERE UserId = 0 AND EXISTS (SELECT 1 FROM Users);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserId",
                table: "Products",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UserId",
                table: "Operations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_UserId",
                table: "Operations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_UserId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Operations_UserId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Operations");
        }
    }
}
