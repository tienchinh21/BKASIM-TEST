using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMembershipGroupForCustomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add HasCustomFieldsSubmitted column to MembershipGroups table
            migrationBuilder.AddColumn<bool>(
                name: "HasCustomFieldsSubmitted",
                table: "MembershipGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Create index for HasCustomFieldsSubmitted
            migrationBuilder.CreateIndex(
                name: "IX_MembershipGroups_HasCustomFieldsSubmitted",
                table: "MembershipGroups",
                column: "HasCustomFieldsSubmitted");

            // Add foreign key constraint from CustomFieldValues to MembershipGroups
            // This allows querying custom field values for a specific membership application
            migrationBuilder.AddForeignKey(
                name: "FK_CustomFieldValues_MembershipGroups_EntityId",
                table: "CustomFieldValues",
                column: "EntityId",
                principalTable: "MembershipGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFieldValues_MembershipGroups_EntityId",
                table: "CustomFieldValues");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_MembershipGroups_HasCustomFieldsSubmitted",
                table: "MembershipGroups");

            // Drop column
            migrationBuilder.DropColumn(
                name: "HasCustomFieldsSubmitted",
                table: "MembershipGroups");
        }
    }
}
