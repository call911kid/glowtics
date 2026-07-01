using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowtics.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductEndpoint",
                table: "Retailers",
                newName: "BrandLogoUrl");

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserId",
                table: "DiagnosticSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "DiagnosticSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                table: "DiagnosticSessions");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "DiagnosticSessions");

            migrationBuilder.RenameColumn(
                name: "BrandLogoUrl",
                table: "Retailers",
                newName: "ProductEndpoint");
        }
    }
}
