using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInterfaceUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetadataUrl",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "SwaggerUrl",
                table: "Applications",
                newName: "InterfaceUrl");

            migrationBuilder.AddColumn<int>(
                name: "InterfaceType",
                table: "Applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Bestehende SwaggerUrl-Einträge als REST markieren (InterfaceType = 1)
            migrationBuilder.Sql("UPDATE Applications SET InterfaceType = 1 WHERE InterfaceUrl IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterfaceType",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "InterfaceUrl",
                table: "Applications",
                newName: "SwaggerUrl");

            migrationBuilder.AddColumn<string>(
                name: "MetadataUrl",
                table: "Applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
