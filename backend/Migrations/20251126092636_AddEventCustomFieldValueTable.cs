using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCustomFieldValueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventCustomFieldValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventCustomFieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestListId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCustomFieldValues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventCustomFieldValues_EventCustomFieldId",
                table: "EventCustomFieldValues",
                column: "EventCustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCustomFieldValues_EventRegistrationId",
                table: "EventCustomFieldValues",
                column: "EventRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCustomFieldValues_GuestListId",
                table: "EventCustomFieldValues",
                column: "GuestListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventCustomFieldValues");
        }
    }
}
