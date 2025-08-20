using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class Update121 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedById", "CreationDate", "DeleteById", "DeletionDate", "IsDeleted", "ModificationById", "ModificationDate", "Name" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), null, null, null, null, false, null, null, "ShopManager" });
        }
    }
}
