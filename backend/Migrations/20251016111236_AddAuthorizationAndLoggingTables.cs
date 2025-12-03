using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizationAndLoggingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetEntity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileShareLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SharerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SharedData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileShareLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferralLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReferrerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RefereeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_AccountId",
                table: "ActivityLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActionType",
                table: "ActivityLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedDate",
                table: "ActivityLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TargetEntity",
                table: "ActivityLogs",
                column: "TargetEntity");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_GroupId",
                table: "GroupPermissions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_IsActive",
                table: "GroupPermissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_UserId",
                table: "GroupPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_UserId_GroupId",
                table: "GroupPermissions",
                columns: new[] { "UserId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileShareLogs_CreatedDate",
                table: "ProfileShareLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileShareLogs_GroupId",
                table: "ProfileShareLogs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileShareLogs_ReceiverId",
                table: "ProfileShareLogs",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileShareLogs_ShareMethod",
                table: "ProfileShareLogs",
                column: "ShareMethod");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileShareLogs_SharerId",
                table: "ProfileShareLogs",
                column: "SharerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_CreatedDate",
                table: "ReferralLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_GroupId",
                table: "ReferralLogs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_RefereeId",
                table: "ReferralLogs",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_ReferralCode",
                table: "ReferralLogs",
                column: "ReferralCode");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_ReferrerId",
                table: "ReferralLogs",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLogs_Source",
                table: "ReferralLogs",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "ProfileShareLogs");

            migrationBuilder.DropTable(
                name: "ReferralLogs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "AspNetUsers");
        }
    }
}
