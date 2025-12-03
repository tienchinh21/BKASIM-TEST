using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class updatefieldsToMembership2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DayOfBirth",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldIds",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "DayOfBirth",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "FieldIds",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Memberships");
        }
    }
}
