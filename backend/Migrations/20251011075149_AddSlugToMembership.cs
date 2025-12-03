using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add OldSlugs (nullable)
            migrationBuilder.AddColumn<string>(
                name: "OldSlugs",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            // Add Slug (nullable, không tạo index)
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Memberships",
                type: "nvarchar(450)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldSlugs",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Memberships");
        }
    }
}
