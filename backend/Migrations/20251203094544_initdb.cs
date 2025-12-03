using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAppGIBA.Migrations
{
    /// <inheritdoc />
    public partial class initdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AdminRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppointmentFrom = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BannerImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    GroupCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderPriority = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BehaviorRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviorRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CampaignId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VariableContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateVariable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagAttribute = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignCSKHs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateCount = table.Column<byte>(type: "tinyint", nullable: false),
                    CampaignStatusID = table.Column<byte>(type: "tinyint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountCmsId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total = table.Column<int>(type: "int", nullable: false),
                    TotalSuccess = table.Column<int>(type: "int", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldScheduleTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCSKHs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampaignKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPhoneCSKHs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CampaignCSKHId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdOmniMess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChannelCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParamContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ReportTimes = table.Column<short>(type: "smallint", nullable: false),
                    UpdateCount = table.Column<short>(type: "smallint", nullable: false),
                    DelayTime = table.Column<int>(type: "int", nullable: false),
                    IsCharged = table.Column<bool>(type: "bit", nullable: false),
                    TelcoID = table.Column<byte>(type: "tinyint", nullable: false),
                    ChannelID = table.Column<short>(type: "smallint", nullable: false),
                    Duration = table.Column<short>(type: "smallint", nullable: false),
                    MtCount = table.Column<short>(type: "smallint", nullable: false),
                    ExtraDuration = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IndexCrDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IndexProcessTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPhoneCSKHs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPhoneCSKHTemps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CampaignCSHKId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    TelcoID = table.Column<byte>(type: "tinyint", nullable: false),
                    RequestID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParamContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPhoneCSKHTemps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TagID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampaignId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldTabs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TabName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldTabs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatasourceETMs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasourceETMs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventCustomFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldType = table.Column<byte>(type: "tinyint", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCustomFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventCustomFieldValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventCustomFieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestListId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCustomFieldValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TelcoId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdOMniMess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParamsContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTemplateLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplateLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateMapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTriggerLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTriggerLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTriggerSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingStep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTriggerSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrderMiniApp = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupPackageConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPackageConfigs", x => x.Id);
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
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorRulesUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainActivities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "HttpConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeetingType = table.Column<byte>(type: "tinyint", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeetingLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserZaloId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserZaloName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserZaloIdByOA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fullname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OldSlugs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZaloAvatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                    table.UniqueConstraint("AK_Memberships_UserZaloId", x => x.UserZaloId);
                });

            migrationBuilder.CreateTable(
                name: "MemberSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MembershipGroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OmniTemplateConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateMapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmniTemplateConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OmniTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoutingRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateMapping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmniTemplates", x => x.Id);
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
                name: "ProfileTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserZaloId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VisibleFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HiddenFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThemeColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileTemplates", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "Showcases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MembershipId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MembershipName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MemberShipAvatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    RoleId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showcases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SponsorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Introduction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebsiteURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipTiers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebHookLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdOmniMess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelcoId = table.Column<int>(type: "int", nullable: false),
                    MtCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZaloTemplateConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateMapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaloTemplateConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZaloTemplateUids",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListParams = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaloTemplateUids", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CustomFieldTabId = table.Column<string>(type: "nvarchar(32)", nullable: true),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FieldType = table.Column<byte>(type: "tinyint", nullable: false),
                    FieldOptions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsProfile = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFields_CustomFieldTabs_CustomFieldTabId",
                        column: x => x.CustomFieldTabId,
                        principalTable: "CustomFieldTabs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldChildren",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ChildName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FieldId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldChildren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldChildren_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Banner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoinCount = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoogleMapURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NeedApproval = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipGroups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserZaloId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasCustomFieldsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipGroups_Memberships_UserZaloId",
                        column: x => x.UserZaloId,
                        principalTable: "Memberships",
                        principalColumn: "UserZaloId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RefFrom = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefTo = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    RefToGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredMemberGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    ReferredMemberId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReferralName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<byte>(type: "tinyint", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refs_Memberships_RefFrom",
                        column: x => x.RefFrom,
                        principalTable: "Memberships",
                        principalColumn: "UserZaloId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refs_Memberships_RefTo",
                        column: x => x.RefTo,
                        principalTable: "Memberships",
                        principalColumn: "UserZaloId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfileCustomFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ProfileTemplateId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileCustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileCustomFields_ProfileTemplates_ProfileTemplateId",
                        column: x => x.ProfileTemplateId,
                        principalTable: "ProfileTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventGifts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    GiftName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventGifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventGifts_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventGuests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    UserZaloId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventGuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventGuests_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventGuests_Memberships_UserZaloId",
                        column: x => x.UserZaloId,
                        principalTable: "Memberships",
                        principalColumn: "UserZaloId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    UserZaloId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckInStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Memberships_UserZaloId",
                        column: x => x.UserZaloId,
                        principalTable: "Memberships",
                        principalColumn: "UserZaloId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventSponsors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    SponsorId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    SponsorshipTierId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSponsors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSponsors_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSponsors_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSponsors_SponsorshipTiers_SponsorshipTierId",
                        column: x => x.SponsorshipTierId,
                        principalTable: "SponsorshipTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CustomFieldId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldValues_CustomFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomFieldValues_MembershipGroups_EntityId",
                        column: x => x.EntityId,
                        principalTable: "MembershipGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestLists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventGuestId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CheckInCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckInStatus = table.Column<bool>(type: "bit", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestLists_EventGuests_EventGuestId",
                        column: x => x.EventGuestId,
                        principalTable: "EventGuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_AdminRoles_IsActive",
                table: "AdminRoles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_Name",
                table: "AdminRoles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentFrom",
                table: "Appointments",
                column: "AppointmentFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentTo",
                table: "Appointments",
                column: "AppointmentTo");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_DisplayOrder",
                table: "ArticleCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_Name",
                table: "ArticleCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_GroupCategory",
                table: "Articles",
                column: "GroupCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_GroupIds",
                table: "Articles",
                column: "GroupIds");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_OrderPriority",
                table: "Articles",
                column: "OrderPriority");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Status",
                table: "Articles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BehaviorRules_GroupId",
                table: "BehaviorRules",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BehaviorRules_IsActive",
                table: "BehaviorRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFields_CustomFieldTabId",
                table: "CustomFields",
                column: "CustomFieldTabId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFields_DisplayOrder",
                table: "CustomFields",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFields_EntityType_EntityId",
                table: "CustomFields",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldTabs_DisplayOrder",
                table: "CustomFieldTabs",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldTabs_EntityType_EntityId",
                table: "CustomFieldTabs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValues_CustomFieldId",
                table: "CustomFieldValues",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValues_EntityId",
                table: "CustomFieldValues",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValues_EntityType_EntityId",
                table: "CustomFieldValues",
                columns: new[] { "EntityType", "EntityId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_EventGifts_EventId",
                table: "EventGifts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventGuests_EventId",
                table: "EventGuests",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventGuests_UserZaloId",
                table: "EventGuests",
                column: "UserZaloId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId",
                table: "EventRegistrations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_UserZaloId",
                table: "EventRegistrations",
                column: "UserZaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_GroupId",
                table: "Events",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_EventId",
                table: "EventSponsors",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_SponsorId",
                table: "EventSponsors",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_SponsorshipTierId",
                table: "EventSponsors",
                column: "SponsorshipTierId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChildren_FieldId",
                table: "FieldChildren",
                column: "FieldId");

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
                name: "IX_GuestLists_EventGuestId",
                table: "GuestLists",
                column: "EventGuestId");

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

            migrationBuilder.CreateIndex(
                name: "IX_MembershipGroups_GroupId",
                table: "MembershipGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipGroups_HasCustomFieldsSubmitted",
                table: "MembershipGroups",
                column: "HasCustomFieldsSubmitted");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipGroups_UserZaloId",
                table: "MembershipGroups",
                column: "UserZaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_RoleId",
                table: "Memberships",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_Slug",
                table: "Memberships",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserZaloId",
                table: "Memberships",
                column: "UserZaloId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileCustomFields_ProfileTemplateId",
                table: "ProfileCustomFields",
                column: "ProfileTemplateId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Refs_ReferredMemberId",
                table: "Refs",
                column: "ReferredMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Refs_RefFrom",
                table: "Refs",
                column: "RefFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Refs_RefTo",
                table: "Refs",
                column: "RefTo");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_EndDate",
                table: "Showcases",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_GroupId",
                table: "Showcases",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_MembershipId",
                table: "Showcases",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_RoleId",
                table: "Showcases",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_StartDate",
                table: "Showcases",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_Status",
                table: "Showcases",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "AdminRoles");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "ArticleCategories");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BehaviorRules");

            migrationBuilder.DropTable(
                name: "CampaignConfigs");

            migrationBuilder.DropTable(
                name: "CampaignCSKHs");

            migrationBuilder.DropTable(
                name: "CampaignItems");

            migrationBuilder.DropTable(
                name: "CampaignPhoneCSKHs");

            migrationBuilder.DropTable(
                name: "CampaignPhoneCSKHTemps");

            migrationBuilder.DropTable(
                name: "CampaignTags");

            migrationBuilder.DropTable(
                name: "Commons");

            migrationBuilder.DropTable(
                name: "CustomFieldValues");

            migrationBuilder.DropTable(
                name: "DatasourceETMs");

            migrationBuilder.DropTable(
                name: "EventCustomFields");

            migrationBuilder.DropTable(
                name: "EventCustomFieldValues");

            migrationBuilder.DropTable(
                name: "EventGifts");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "EventRegistrations");

            migrationBuilder.DropTable(
                name: "EventSponsors");

            migrationBuilder.DropTable(
                name: "EventTemplateLogs");

            migrationBuilder.DropTable(
                name: "EventTemplates");

            migrationBuilder.DropTable(
                name: "EventTriggerLogs");

            migrationBuilder.DropTable(
                name: "EventTriggerSettings");

            migrationBuilder.DropTable(
                name: "FieldChildren");

            migrationBuilder.DropTable(
                name: "GroupPackageConfigs");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "GuestLists");

            migrationBuilder.DropTable(
                name: "HomePins");

            migrationBuilder.DropTable(
                name: "HttpConfigs");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropTable(
                name: "MemberSubscriptions");

            migrationBuilder.DropTable(
                name: "OmniTemplateConfigs");

            migrationBuilder.DropTable(
                name: "OmniTemplates");

            migrationBuilder.DropTable(
                name: "ProfileCustomFields");

            migrationBuilder.DropTable(
                name: "ProfileShareLogs");

            migrationBuilder.DropTable(
                name: "ReferralLogs");

            migrationBuilder.DropTable(
                name: "Refs");

            migrationBuilder.DropTable(
                name: "Showcases");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "WebHookLogs");

            migrationBuilder.DropTable(
                name: "ZaloTemplateConfigs");

            migrationBuilder.DropTable(
                name: "ZaloTemplateUids");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CustomFields");

            migrationBuilder.DropTable(
                name: "MembershipGroups");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "SponsorshipTiers");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "EventGuests");

            migrationBuilder.DropTable(
                name: "ProfileTemplates");

            migrationBuilder.DropTable(
                name: "CustomFieldTabs");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
