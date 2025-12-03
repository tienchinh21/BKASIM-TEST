using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldChildDisplayOrderAndIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FieldChildren_DisplayOrder",
                table: "FieldChildren");

            migrationBuilder.DropIndex(
                name: "IX_FieldChildren_IsActive",
                table: "FieldChildren");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "FieldChildren");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FieldChildren");

            migrationBuilder.AlterColumn<string>(
                name: "FieldId",
                table: "FieldChildren",
                type: "nvarchar(32)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldChildren_Fields_FieldId",
                table: "FieldChildren",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldChildren_Fields_FieldId",
                table: "FieldChildren");

            migrationBuilder.AlterColumn<string>(
                name: "FieldId",
                table: "FieldChildren",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "FieldChildren",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FieldChildren",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_DisplayOrder",
                table: "FieldChildren",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_IsActive",
                table: "FieldChildren",
                column: "IsActive");
        }
    }
}
