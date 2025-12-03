using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroupTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove Type column from Groups table
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add Type column to Groups table (for rollback)
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
