using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnsToRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GroupId",
                table: "Refs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralAddress",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralEmail",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralName",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralPhone",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredMemberId",
                table: "Refs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredMemberSnapshot",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refs_GroupId",
                table: "Refs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Refs_ReferredMemberId",
                table: "Refs",
                column: "ReferredMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Refs_GroupId",
                table: "Refs");

            migrationBuilder.DropIndex(
                name: "IX_Refs_ReferredMemberId",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferralAddress",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferralEmail",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferralName",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferralPhone",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferredMemberId",
                table: "Refs");

            migrationBuilder.DropColumn(
                name: "ReferredMemberSnapshot",
                table: "Refs");

            migrationBuilder.AlterColumn<string>(
                name: "GroupId",
                table: "Refs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
