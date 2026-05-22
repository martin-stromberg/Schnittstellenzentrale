using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteEndpointGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endpoints_EndpointGroups_EndpointGroupId",
                table: "Endpoints");

            migrationBuilder.AddForeignKey(
                name: "FK_Endpoints_EndpointGroups_EndpointGroupId",
                table: "Endpoints",
                column: "EndpointGroupId",
                principalTable: "EndpointGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endpoints_EndpointGroups_EndpointGroupId",
                table: "Endpoints");

            migrationBuilder.AddForeignKey(
                name: "FK_Endpoints_EndpointGroups_EndpointGroupId",
                table: "Endpoints",
                column: "EndpointGroupId",
                principalTable: "EndpointGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
