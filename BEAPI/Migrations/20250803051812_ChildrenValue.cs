using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChildrenValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Values_ListOfValues_ListOfValueId",
                table: "Values");

            migrationBuilder.AddColumn<Guid>(
                name: "ChildListOfValueId",
                table: "Values",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Values_ChildListOfValueId",
                table: "Values",
                column: "ChildListOfValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Values_ListOfValues_ChildListOfValueId",
                table: "Values",
                column: "ChildListOfValueId",
                principalTable: "ListOfValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Values_ListOfValues_ListOfValueId",
                table: "Values",
                column: "ListOfValueId",
                principalTable: "ListOfValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Values_ListOfValues_ChildListOfValueId",
                table: "Values");

            migrationBuilder.DropForeignKey(
                name: "FK_Values_ListOfValues_ListOfValueId",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Values_ChildListOfValueId",
                table: "Values");

            migrationBuilder.DropColumn(
                name: "ChildListOfValueId",
                table: "Values");

            migrationBuilder.AddForeignKey(
                name: "FK_Values_ListOfValues_ListOfValueId",
                table: "Values",
                column: "ListOfValueId",
                principalTable: "ListOfValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
