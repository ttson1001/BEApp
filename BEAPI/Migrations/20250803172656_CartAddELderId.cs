using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class CartAddELderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Carts",
                newName: "ElderId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                newName: "IX_Carts_ElderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_ElderId",
                table: "Carts",
                column: "ElderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_ElderId",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "ElderId",
                table: "Carts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_ElderId",
                table: "Carts",
                newName: "IX_Carts_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
