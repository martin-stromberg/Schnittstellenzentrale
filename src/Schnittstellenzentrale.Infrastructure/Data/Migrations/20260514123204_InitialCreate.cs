using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SwaggerUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MetadataUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ApplicationGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_ApplicationGroups_ApplicationGroupId",
                        column: x => x.ApplicationGroupId,
                        principalTable: "ApplicationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EndpointGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointGroups_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    AuthenticationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    EndpointGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Endpoints_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Endpoints_EndpointGroups_EndpointGroupId",
                        column: x => x.EndpointGroupId,
                        principalTable: "EndpointGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EndpointHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointHeaders_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndpointQueryParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointQueryParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointQueryParameters_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationGroupId",
                table: "Applications",
                column: "ApplicationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EndpointGroups_ApplicationId",
                table: "EndpointGroups",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EndpointHeaders_EndpointId",
                table: "EndpointHeaders",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_EndpointQueryParameters_EndpointId",
                table: "EndpointQueryParameters",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_Endpoints_ApplicationId",
                table: "Endpoints",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Endpoints_EndpointGroupId",
                table: "Endpoints",
                column: "EndpointGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndpointHeaders");

            migrationBuilder.DropTable(
                name: "EndpointQueryParameters");

            migrationBuilder.DropTable(
                name: "Endpoints");

            migrationBuilder.DropTable(
                name: "EndpointGroups");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "ApplicationGroups");
        }
    }
}
