using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class DeleteField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "AppPosition",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ApprovalReason",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Memberships");

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
                name: "CareAbout",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Company",
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
                name: "Contribute",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "DayOfBirth",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "FieldIds",
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
                name: "Message",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Object",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "OtherContribute",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "SortField",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TotalRatings",
                table: "Memberships");

            migrationBuilder.AddColumn<string>(
                name: "FieldName",
                table: "EventCustomFieldValues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePins");

            migrationBuilder.DropColumn(
                name: "FieldName",
                table: "EventCustomFieldValues");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppPosition",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalReason",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ApprovalStatus",
                table: "Memberships",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Memberships",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
                name: "CareAbout",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Company",
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
                name: "Contribute",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DayOfBirth",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldIds",
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
                name: "Message",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Object",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherContribute",
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

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortField",
                table: "Memberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Term",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRatings",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
