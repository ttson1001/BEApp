using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Consultant",
                table: "UserConnections",
                newName: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_ConsultantId",
                table: "UserConnections",
                column: "ConsultantId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConnections_Users_ConsultantId",
                table: "UserConnections",
                column: "ConsultantId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConnections_Users_ConsultantId",
                table: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_ConsultantId",
                table: "UserConnections");

            migrationBuilder.RenameColumn(
                name: "ConsultantId",
                table: "UserConnections",
                newName: "Consultant");
        }
    }
}
