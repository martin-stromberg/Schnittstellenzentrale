using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemEnvironments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemEnvironments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemEnvironments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    IsValueMasked = table.Column<bool>(type: "INTEGER", nullable: false),
                    SystemEnvironmentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvironmentVariables_SystemEnvironments_SystemEnvironmentId",
                        column: x => x.SystemEnvironmentId,
                        principalTable: "SystemEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentVariables_Name_SystemEnvironmentId",
                table: "EnvironmentVariables",
                columns: new[] { "Name", "SystemEnvironmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentVariables_SystemEnvironmentId",
                table: "EnvironmentVariables",
                column: "SystemEnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEnvironments_Name_Mode_Owner",
                table: "SystemEnvironments",
                columns: new[] { "Name", "Mode", "Owner" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvironmentVariables");

            migrationBuilder.DropTable(
                name: "SystemEnvironments");
        }
    }
}
