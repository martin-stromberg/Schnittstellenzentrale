using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemToApplicationGroupAndApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "ApplicationGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "ApplicationGroups");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Applications");
        }
    }
}
