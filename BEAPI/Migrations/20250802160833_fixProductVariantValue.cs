using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class fixProductVariantValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VauleId",
                table: "ProductVariantValues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VauleId",
                table: "ProductVariantValues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
