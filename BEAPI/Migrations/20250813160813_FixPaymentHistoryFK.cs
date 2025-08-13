using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentHistoryFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Users_UserId1",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_UserId1",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PaymentHistories");

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Amount", "CreatedById", "CreationDate", "DeleteById", "DeletionDate", "IsDeleted", "ModificationById", "ModificationDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777001"), 0m, null, null, null, null, false, null, null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("77777777-7777-7777-7777-777777777002"), 0m, null, null, null, null, false, null, null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("77777777-7777-7777-7777-777777777003"), 0m, null, null, null, null, false, null, null, new Guid("33333333-3333-3333-3333-333333333333") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777001"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777002"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777003"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PaymentHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_UserId1",
                table: "PaymentHistories",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Users_UserId1",
                table: "PaymentHistories",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
