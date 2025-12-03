using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroupIdFromRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Refs_GroupId",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Refs");

            migrationBuilder.RenameColumn(
                name: "ReferredMemberSnapshot",
                table: "Refs",
                newName: "ReferredMemberGroupId");

            migrationBuilder.AddColumn<string>(
                name: "RefToGroupId",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefToGroupId",
                table: "Refs");

            migrationBuilder.RenameColumn(
                name: "ReferredMemberGroupId",
                table: "Refs",
                newName: "ReferredMemberSnapshot");

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Refs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refs_GroupId",
                table: "Refs",
                column: "GroupId");
        }
    }
}
