using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationGroupDescriptionSubtitleIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ApplicationGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "IconData",
                table: "ApplicationGroups",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "ApplicationGroups",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ApplicationGroups");

            migrationBuilder.DropColumn(
                name: "IconData",
                table: "ApplicationGroups");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "ApplicationGroups");
        }
    }
}
