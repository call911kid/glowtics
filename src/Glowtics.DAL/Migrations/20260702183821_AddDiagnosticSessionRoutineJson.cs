using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowtics.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticSessionRoutineJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoutineJson",
                table: "DiagnosticSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkinProfileJson",
                table: "DiagnosticSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "bde4ddad-0bb3-46b4-b51d-cf2bd85cf069");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutineJson",
                table: "DiagnosticSessions");

            migrationBuilder.DropColumn(
                name: "SkinProfileJson",
                table: "DiagnosticSessions");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "862cc04a-062a-4866-927f-401f75445b3d");
        }
    }
}
