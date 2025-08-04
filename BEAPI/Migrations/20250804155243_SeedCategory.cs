using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ListOfValues",
                columns: new[] { "Id", "CreatedById", "CreationDate", "DeleteById", "DeletionDate", "IsDeleted", "Label", "ModificationById", "ModificationDate", "Note", "Type" },
                values: new object[] { new Guid("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"), null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, false, "Loại sản Phẩm", null, null, "CATEGORY", 0 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Name",
                value: "Guardian");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ListOfValues",
                keyColumn: "Id",
                keyValue: new Guid("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Name",
                value: "User");
        }
    }
}
