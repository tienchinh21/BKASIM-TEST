using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldsToFieldChildRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "Fields",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrderMiniApp",
                table: "Fields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FieldChildren",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ChildName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldChildren", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_DisplayOrder",
                table: "FieldChildren",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_FieldId",
                table: "FieldChildren",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_IsActive",
                table: "FieldChildren",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldChildren");

            migrationBuilder.DropColumn(
                name: "DisplayOrderMiniApp",
                table: "Fields");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Fields",
                newName: "ParentId");
        }
    }
}
