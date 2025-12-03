using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToRefTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessField",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BusinessRegistrationDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationPlace",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyBrandName",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyFullName",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogo",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhoneNumber",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyWebsite",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadquartersAddress",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentative",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativePosition",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessField",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationDate",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationPlace",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyBrandName",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyFullName",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyLogo",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyPhoneNumber",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CompanyWebsite",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "HeadquartersAddress",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "LegalRepresentative",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativePosition",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Memberships");
        }
    }
}
