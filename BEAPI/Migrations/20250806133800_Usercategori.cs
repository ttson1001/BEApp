using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class Usercategori : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCategoryValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModificationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificationById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleteById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryValue_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCategoryValue_Values_ValueId",
                        column: x => x.ValueId,
                        principalTable: "Values",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ListOfValues",
                keyColumn: "Id",
                keyValue: new Guid("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"),
                column: "Label",
                value: "Loại sản phẩm");

            migrationBuilder.InsertData(
                table: "ListOfValues",
                columns: new[] { "Id", "CreatedById", "CreationDate", "DeleteById", "DeletionDate", "IsDeleted", "Label", "ModificationById", "ModificationDate", "Note", "Type" },
                values: new object[,]
                {
                    { new Guid("a23f89a1-2c34-4b2d-9876-08dcb9a3abcd"), null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, false, "Thương hiệu", null, null, "BRAND", 2 },
                    { new Guid("c47fabcd-77f2-4f55-8322-08dcb9a3cdef"), null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, false, "Mối quan hệ", null, null, "RELATIONSHIP", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryValue_UserId",
                table: "UserCategoryValue",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryValue_ValueId",
                table: "UserCategoryValue",
                column: "ValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCategoryValue");

            migrationBuilder.DeleteData(
                table: "ListOfValues",
                keyColumn: "Id",
                keyValue: new Guid("a23f89a1-2c34-4b2d-9876-08dcb9a3abcd"));

            migrationBuilder.DeleteData(
                table: "ListOfValues",
                keyColumn: "Id",
                keyValue: new Guid("c47fabcd-77f2-4f55-8322-08dcb9a3cdef"));

            migrationBuilder.UpdateData(
                table: "ListOfValues",
                keyColumn: "Id",
                keyValue: new Guid("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"),
                column: "Label",
                value: "Loại sản Phẩm");
        }
    }
}
