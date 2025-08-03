using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class seeddataUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Age", "Avatar", "BirthDate", "CreatedById", "CreationDate", "DeleteById", "DeletionDate", "Description", "Email", "FullName", "Gender", "GuardianId", "IsDeleted", "ModificationById", "ModificationDate", "OTPId", "PasswordHash", "PhoneNumber", "RefreshToken", "RoleId", "UserName" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 25, null, null, null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "vana@example.com", "Nguyen Van A", "Male", null, false, null, null, null, "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", "0901234567", null, new Guid("11111111-1111-1111-1111-111111111111"), "nguyenvana" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 35, null, null, null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "admin@example.com", "Admin System", "Male", null, false, null, null, null, "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", "0912345678", null, new Guid("33333333-3333-3333-3333-333333333333"), "admin1" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 30, null, null, null, new DateTimeOffset(new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "thib@example.com", "Tran Thi B", "Female", new Guid("11111111-1111-1111-1111-111111111111"), false, null, null, null, "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", "0909876543", null, new Guid("22222222-2222-2222-2222-222222222222"), "tranthib" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
