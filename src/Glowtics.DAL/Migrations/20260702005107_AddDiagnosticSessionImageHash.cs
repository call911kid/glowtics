using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowtics.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticSessionImageHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiagnosticSessions_RetailerId",
                table: "DiagnosticSessions");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalUserId",
                table: "DiagnosticSessions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageHash",
                table: "DiagnosticSessions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "862cc04a-062a-4866-927f-401f75445b3d");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticSessions_RetailerId_ExternalUserId_ImageHash",
                table: "DiagnosticSessions",
                columns: new[] { "RetailerId", "ExternalUserId", "ImageHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiagnosticSessions_RetailerId_ExternalUserId_ImageHash",
                table: "DiagnosticSessions");

            migrationBuilder.DropColumn(
                name: "ImageHash",
                table: "DiagnosticSessions");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalUserId",
                table: "DiagnosticSessions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticSessions_RetailerId",
                table: "DiagnosticSessions",
                column: "RetailerId");
        }
    }
}
