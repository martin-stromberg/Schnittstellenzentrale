using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schnittstellenzentrale.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentGroupIdToEndpointGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentGroupId",
                table: "EndpointGroups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EndpointGroups_ParentGroupId",
                table: "EndpointGroups",
                column: "ParentGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EndpointGroups_ParentGroupId",
                table: "EndpointGroups");

            migrationBuilder.DropColumn(
                name: "ParentGroupId",
                table: "EndpointGroups");
        }
    }
}
