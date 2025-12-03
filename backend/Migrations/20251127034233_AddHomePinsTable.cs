using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePinsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomePins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    PinnedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PinnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomePins_DisplayOrder",
                table: "HomePins",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_HomePins_EntityType",
                table: "HomePins",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_HomePins_IsActive",
                table: "HomePins",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HomePins_PinnedBy",
                table: "HomePins",
                column: "PinnedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_HomePins_Entity",
                table: "HomePins",
                columns: new[] { "EntityType", "EntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePins");
        }
    }
}
