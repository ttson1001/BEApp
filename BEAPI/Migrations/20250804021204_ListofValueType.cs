using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class ListofValueType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Values",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ListOfValues",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Values");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ListOfValues");
        }
    }
}
