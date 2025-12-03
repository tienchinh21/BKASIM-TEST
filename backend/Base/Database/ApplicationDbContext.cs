
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Fields;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Notifications;
using MiniAppGIBA.Entities.OmniTool;
using MiniAppGIBA.Entities.Sponsors;
using MiniAppGIBA.Entities.Subscriptions;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Entities.Showcase;
using MiniAppGIBA.Entities.Appointment;
using MiniAppGIBA.Entities.Rules;
using MiniAppGIBA.Entities.Meetings;
using MiniAppGIBA.Entities.HomePins;
using MiniAppGIBA.Entities.CustomFields;

namespace MiniAppGIBA.Base.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Articles
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<ArticleCategory> ArticleCategories { get; set; }
        #endregion

        #region Commons
        public virtual DbSet<Ref> Refs { get; set; }
        public virtual DbSet<SystemConfig> SystemConfigs { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<MiniAppGIBA.Entities.Commons.Common> Commons { get; set; }
        #endregion

        #region Fields
        public virtual DbSet<Field> Fields { get; set; }
        public virtual DbSet<FieldChild> FieldChildren { get; set; }
        #endregion

        #region Memberships
        public virtual DbSet<Membership> Memberships { get; set; }
        public virtual DbSet<ProfileTemplate> ProfileTemplates { get; set; }
        public virtual DbSet<ProfileCustomField> ProfileCustomFields { get; set; }
        #endregion

        #region Groups
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<MembershipGroup> MembershipGroups { get; set; }
        #endregion

        #region Events
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<EventRegistration> EventRegistrations { get; set; }
        public virtual DbSet<EventGuest> EventGuests { get; set; }
        public virtual DbSet<GuestList> GuestLists { get; set; }
        public virtual DbSet<EventGift> EventGifts { get; set; }
        public virtual DbSet<EventCustomField> EventCustomFields { get; set; }
        public virtual DbSet<EventCustomFieldValue> EventCustomFieldValues { get; set; }
        #endregion

        #region Showcase
        public virtual DbSet<Showcase> Showcases { get; set; }
        #endregion

        #region Meetings
        public virtual DbSet<Meeting> Meetings { get; set; }
        #endregion

        #region Sponsors
        public virtual DbSet<Sponsor> Sponsors { get; set; }
        public virtual DbSet<SponsorshipTier> SponsorshipTiers { get; set; }
        public virtual DbSet<EventSponsor> EventSponsors { get; set; }
        #endregion

        #region Subscriptions
        public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public virtual DbSet<GroupPackageConfig> GroupPackageConfigs { get; set; }
        public virtual DbSet<MemberSubscription> MemberSubscriptions { get; set; }
        #endregion

        #region BehaviorRules
        public virtual DbSet<BehaviorRule> BehaviorRules { get; set; }
        #endregion


        #region Admins
        public virtual DbSet<GroupPermission> GroupPermissions { get; set; }
        public virtual DbSet<Roles> AdminRoles { get; set; }
        #endregion

        #region Logs
        public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
        public virtual DbSet<ReferralLog> ReferralLogs { get; set; }
        public virtual DbSet<ProfileShareLog> ProfileShareLogs { get; set; }
        #endregion

        #region HomePins
        public virtual DbSet<HomePin> HomePins { get; set; }
        #endregion

        #region CustomFields
        public virtual DbSet<CustomFieldTab> CustomFieldTabs { get; set; }
        public virtual DbSet<CustomField> CustomFields { get; set; }
        public virtual DbSet<CustomFieldValue> CustomFieldValues { get; set; }
        #endregion

        #region Omni Mini Tools

        public virtual DbSet<WebHookLogs> WebHookLogs { get; set; }
        public virtual DbSet<CampaignTag> CampaignTags { get; set; }
        public virtual DbSet<CampaignCSKH> CampaignCSKHs { get; set; }
        public virtual DbSet<CampaignConfig> CampaignConfigs { get; set; }
        public virtual DbSet<CampaignPhoneCSKH> CampaignPhoneCSKHs { get; set; }
        public virtual DbSet<CampaignPhoneCSKHTemp> CampaignPhoneCSKHTemps { get; set; }

        public virtual DbSet<EventLog> EventLogs { get; set; }
        public virtual DbSet<EventTemplate> EventTemplates { get; set; } // lưu cấu hình chung của thằng Event Template
        public virtual DbSet<EventTemplateLog> EventTemplateLogs { get; set; } // Log event template đã gửi đi

        public virtual DbSet<HttpConfig> HttpConfigs { get; set; } // config riêng dành cho http request
        public virtual DbSet<OmniTemplateConfig> OmniTemplateConfigs { get; set; } // config riêng dành cho Omni
        public virtual DbSet<ZaloTemplateConfig> ZaloTemplateConfigs { get; set; } // config riêng dành cho UID

        public virtual DbSet<ZaloTemplateUid> ZaloTemplateUids { get; set; } // Lưu mẫu template dành cho UID

        public virtual DbSet<CampaignItem> CampaignItems { get; set; } // Voucher Code

        public DbSet<DatasourceETM> DatasourceETMs { get; set; }
        public DbSet<EventTriggerLog> EventTriggerLogs { get; set; }
        public DbSet<EventTriggerSetting> EventTriggerSettings { get; set; }
        public DbSet<Entities.Notifications.Templates.OmniTemplate> OmniTemplates { get; set; }

        #endregion

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // Configure unique constraints
            modelBuilder.Entity<Membership>()
                .HasIndex(m => m.UserZaloId)
                .IsUnique();

            modelBuilder.Entity<Membership>()
                .HasIndex(m => m.Slug)
                .IsUnique();

            // Configure relationships

            // Membership 1-n MembershipGroup
            modelBuilder.Entity<MembershipGroup>()
                .HasOne(mg => mg.Membership)
                .WithMany(m => m.MembershipGroups)
                .HasForeignKey(mg => mg.UserZaloId)
                .HasPrincipalKey(m => m.UserZaloId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group 1-n MembershipGroup
            modelBuilder.Entity<MembershipGroup>()
                .HasOne(mg => mg.Group)
                .WithMany(g => g.MembershipGroups)
                .HasForeignKey(mg => mg.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group 1-n Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Events)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event 1-n EventRegistration
            modelBuilder.Entity<EventRegistration>()
                .HasOne(er => er.Event)
                .WithMany(e => e.EventRegistrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership 1-n EventRegistration
            modelBuilder.Entity<EventRegistration>()
                .HasOne(er => er.Membership)
                .WithMany(m => m.EventRegistrations)
                .HasForeignKey(er => er.UserZaloId)
                .HasPrincipalKey(m => m.UserZaloId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event 1-n EventGuest
            modelBuilder.Entity<EventGuest>()
                .HasOne(eg => eg.Event)
                .WithMany(e => e.EventGuests)
                .HasForeignKey(eg => eg.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventGuest 1-n GuestList
            modelBuilder.Entity<GuestList>()
                .HasOne(gl => gl.EventGuest)
                .WithMany(eg => eg.GuestLists)
                .HasForeignKey(gl => gl.EventGuestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership 1-n EventGuest
            modelBuilder.Entity<EventGuest>()
                .HasOne(eg => eg.Membership)
                .WithMany(m => m.EventGuests)
                .HasForeignKey(eg => eg.UserZaloId)
                .HasPrincipalKey(m => m.UserZaloId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event 1-n EventGift
            modelBuilder.Entity<EventGift>()
                .HasOne(eg => eg.Event)
                .WithMany(e => e.EventGifts)
                .HasForeignKey(eg => eg.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for EventCustomFieldValue (không dùng navigation, chỉ index)
            modelBuilder.Entity<EventCustomFieldValue>(entity =>
            {
                entity.HasIndex(ecfv => ecfv.EventCustomFieldId);
                entity.HasIndex(ecfv => ecfv.EventRegistrationId);
                entity.HasIndex(ecfv => ecfv.GuestListId);
            });

            // Event 1-n EventSponsor
            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.Event)
                .WithMany(e => e.EventSponsors)
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sponsor 1-n EventSponsor
            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.Sponsor)
                .WithMany(s => s.EventSponsors)
                .HasForeignKey(es => es.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);

            // SponsorshipTier 1-n EventSponsor
            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.SponsorshipTier)
                .WithMany(st => st.EventSponsors)
                .HasForeignKey(es => es.SponsorshipTierId)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership 1-n Ref (FromMember)
            modelBuilder.Entity<Ref>()
                .HasOne(r => r.FromMember)
                .WithMany()
                .HasForeignKey(r => r.RefFrom)
                .HasPrincipalKey(m => m.UserZaloId)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership 1-n Ref (ToMember)
            modelBuilder.Entity<Ref>()
                .HasOne(r => r.ToMember)
                .WithMany()
                .HasForeignKey(r => r.RefTo)
                .HasPrincipalKey(m => m.UserZaloId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ref>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.FromMember)
                    .WithMany() // Nếu Membership khôFng có collection RefFroms

                .OnDelete(DeleteBehavior.Restrict); // ✅ Tắt cascade delete

                entity.HasOne(r => r.ToMember)
                    .WithMany() // Nếu Membership không có collection RefTos
                    .OnDelete(DeleteBehavior.Restrict); // ✅ Tắt cascade delete
                entity.HasIndex(r => r.ReferredMemberId);

            });

            // ========================================
            // ✅ FOREIGN KEY CONFIGURATIONS - NEW TABLES
            // ========================================

            // GroupPermissions: UserId -> AspNetUsers.Id, GroupId -> Groups.Id
            modelBuilder.Entity<GroupPermission>(entity =>
            {
                entity.HasKey(gp => gp.Id);

                // FK: UserId -> AspNetUsers.Id
                entity.HasIndex(gp => gp.UserId);
                entity.Property(gp => gp.UserId).IsRequired();

                // FK: GroupId -> Groups.Id
                entity.HasIndex(gp => gp.GroupId);
                entity.Property(gp => gp.GroupId).IsRequired();

                // Unique constraint: (UserId, GroupId)
                entity.HasIndex(gp => new { gp.UserId, gp.GroupId }).IsUnique();

                // Index for IsActive
                entity.HasIndex(gp => gp.IsActive);
            });

            // ActivityLogs: AccountId -> AspNetUsers.Id
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(al => al.Id);

                // FK: AccountId -> AspNetUsers.Id
                entity.HasIndex(al => al.AccountId);
                entity.Property(al => al.AccountId).IsRequired();

                // Indexes for filtering
                entity.HasIndex(al => al.ActionType);
                entity.HasIndex(al => al.TargetEntity);
                entity.HasIndex(al => al.CreatedDate);
            });

            // ReferralLogs: ReferrerId -> Memberships.UserZaloId, RefereeId -> Memberships.UserZaloId, GroupId -> Groups.Id
            modelBuilder.Entity<ReferralLog>(entity =>
            {
                entity.HasKey(rl => rl.Id);

                // FK: ReferrerId -> Memberships.UserZaloId
                entity.HasIndex(rl => rl.ReferrerId);
                entity.Property(rl => rl.ReferrerId).IsRequired();

                // FK: RefereeId -> Memberships.UserZaloId
                entity.HasIndex(rl => rl.RefereeId);
                entity.Property(rl => rl.RefereeId).IsRequired();

                // FK: GroupId -> Groups.Id
                entity.HasIndex(rl => rl.GroupId);
                entity.Property(rl => rl.GroupId).IsRequired();

                // Indexes for filtering
                entity.HasIndex(rl => rl.ReferralCode);
                entity.HasIndex(rl => rl.Source);
                entity.HasIndex(rl => rl.CreatedDate);
            });

            // ProfileShareLogs: SharerId -> Memberships.UserZaloId, ReceiverId -> Memberships.UserZaloId, GroupId -> Groups.Id
            modelBuilder.Entity<ProfileShareLog>(entity =>
            {
                entity.HasKey(psl => psl.Id);

                // FK: SharerId -> Memberships.UserZaloId
                entity.HasIndex(psl => psl.SharerId);
                entity.Property(psl => psl.SharerId).IsRequired();

                // FK: ReceiverId -> Memberships.UserZaloId
                entity.HasIndex(psl => psl.ReceiverId);
                entity.Property(psl => psl.ReceiverId).IsRequired();

                // FK: GroupId -> Groups.Id
                entity.HasIndex(psl => psl.GroupId);
                entity.Property(psl => psl.GroupId).IsRequired();

                // Indexes for filtering
                entity.HasIndex(psl => psl.ShareMethod);
                entity.HasIndex(psl => psl.CreatedDate);
            });

            // Field 1-n FieldChild
            modelBuilder.Entity<FieldChild>(entity =>
            {
                entity.HasKey(fc => fc.Id);

                // Relationship
                entity.HasOne<Field>()
                    .WithMany(f => f.Children)
                    .HasForeignKey(fc => fc.FieldId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(fc => fc.FieldId);

                // Properties
                entity.Property(fc => fc.ChildName).IsRequired().HasMaxLength(255);
                entity.Property(fc => fc.Description).HasMaxLength(1000);
                entity.Property(fc => fc.FieldId).IsRequired();
            });

            // ========================================
            // ✅ ROLES AND MEMBERSHIP RELATIONSHIPS
            // ========================================

            // Roles configuration
            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(r => r.Id);

                // Indexes
                entity.HasIndex(r => r.Name);
                entity.HasIndex(r => r.IsActive);

                // Properties
                entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
            });

            // Membership n-1 Roles (Many-to-One relationship)
            // FK: Membership.RoleId -> Roles.Id (không dùng navigation property)
            modelBuilder.Entity<Membership>(entity =>
            {
                // Foreign Key configuration
                entity.Property(m => m.RoleId)
                    .HasMaxLength(32);

                // Index for RoleId
                entity.HasIndex(m => m.RoleId);
            });

            // ========================================
            // ✅ ARTICLES CONFIGURATION
            // ========================================

            // ArticleCategory configuration
            modelBuilder.Entity<ArticleCategory>(entity =>
            {
                entity.HasKey(ac => ac.Id);

                // Indexes
                entity.HasIndex(ac => ac.Name);
                entity.HasIndex(ac => ac.DisplayOrder);

                // Properties
                entity.Property(ac => ac.Name).IsRequired().HasMaxLength(255);
            });

            // Article configuration
            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(a => a.Id);

                // Foreign Key: Article.CategoryId -> ArticleCategory.Id (không dùng navigation property)
                entity.Property(a => a.CategoryId).HasMaxLength(32);

                // Indexes for filtering and sorting
                entity.HasIndex(a => a.CategoryId);
                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => a.OrderPriority);
                entity.HasIndex(a => a.GroupCategory);
                entity.HasIndex(a => a.GroupIds); // Index for multi-group filtering

                // Properties
                entity.Property(a => a.BannerImage).IsRequired().HasMaxLength(500);
                entity.Property(a => a.Title).IsRequired().HasMaxLength(500);
                entity.Property(a => a.Author).IsRequired().HasMaxLength(255);
                entity.Property(a => a.Content).IsRequired();
                entity.Property(a => a.Images).HasMaxLength(2000);
                entity.Property(a => a.GroupCategory).HasMaxLength(100);
                entity.Property(a => a.GroupIds).HasMaxLength(500); // Comma-separated group IDs
            });

            // Showcase configuration
            modelBuilder.Entity<Showcase>(entity =>
            {
                entity.HasKey(s => s.Id);

                // Indexes
                entity.HasIndex(s => s.GroupId);
                entity.HasIndex(s => s.MembershipId);
                entity.HasIndex(s => s.RoleId);
                entity.HasIndex(s => s.Status);
                entity.HasIndex(s => s.StartDate);
                entity.HasIndex(s => s.EndDate);

                // Properties
                entity.Property(s => s.GroupId).IsRequired().HasMaxLength(32);
                entity.Property(s => s.GroupName).HasMaxLength(255);
                entity.Property(s => s.Title).IsRequired().HasMaxLength(500);
                entity.Property(s => s.Description).HasMaxLength(2000);
                entity.Property(s => s.MembershipId).IsRequired().HasMaxLength(32);
                entity.Property(s => s.MembershipName).HasMaxLength(255);
                entity.Property(s => s.Location).HasMaxLength(500);
                entity.Property(s => s.Status).HasDefaultValue((byte)1);
                entity.Property(s => s.RoleId).IsRequired().HasMaxLength(32);
                entity.Property(s => s.CreatedBy).HasMaxLength(32);
            });

            // Appointment configuration
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);

                // Indexes
                entity.HasIndex(a => a.AppointmentFrom);
                entity.Property(a => a.AppointmentFrom).IsRequired();

                entity.HasIndex(a => a.AppointmentTo);
                entity.Property(a => a.AppointmentTo).IsRequired();

                entity.HasIndex(a => a.Status);

                // Properties
                entity.Property(a => a.Name).IsRequired().HasMaxLength(255);
            });

            // ========================================
            // ✅ RULES CONFIGURATION
            // ========================================

            // BehaviorRule configuration
            modelBuilder.Entity<BehaviorRule>(entity =>
            {
                entity.HasKey(r => r.Id);

                // Indexes
                entity.HasIndex(r => r.IsActive);
                entity.HasIndex(r => r.GroupId);
            });

            // ========================================
            // ✅ HOME PINS CONFIGURATION
            // ========================================
            modelBuilder.ApplyConfiguration(new HomePinConfiguration());

            // ========================================
            // ✅ CUSTOM FIELDS CONFIGURATION
            // ========================================

            // CustomFieldTab configuration
            modelBuilder.Entity<CustomFieldTab>(entity =>
            {
                entity.HasKey(t => t.Id);

                // Indexes for filtering and sorting
                entity.HasIndex(t => new { t.EntityType, t.EntityId });
                entity.HasIndex(t => t.DisplayOrder);

                // Properties
                entity.Property(t => t.EntityId).IsRequired().HasMaxLength(32);
                entity.Property(t => t.TabName).IsRequired().HasMaxLength(255);
                entity.Property(t => t.DisplayOrder).HasDefaultValue(0);
            });

            // CustomField configuration
            modelBuilder.Entity<CustomField>(entity =>
            {
                entity.HasKey(f => f.Id);

                // Indexes for filtering and sorting
                entity.HasIndex(f => f.CustomFieldTabId);
                entity.HasIndex(f => new { f.EntityType, f.EntityId });
                entity.HasIndex(f => f.DisplayOrder);

                // Properties
                entity.Property(f => f.EntityId).IsRequired().HasMaxLength(32);
                entity.Property(f => f.FieldName).IsRequired().HasMaxLength(255);
                entity.Property(f => f.FieldOptions).HasMaxLength(2000); // JSON options
                entity.Property(f => f.DisplayOrder).HasDefaultValue(0);

                // Relationship: CustomField -> CustomFieldTab
                entity.HasOne(f => f.CustomFieldTab)
                    .WithMany(t => t.CustomFields)
                    .HasForeignKey(f => f.CustomFieldTabId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CustomFieldValue configuration
            modelBuilder.Entity<CustomFieldValue>(entity =>
            {
                entity.HasKey(v => v.Id);

                // Indexes for filtering
                entity.HasIndex(v => v.CustomFieldId);
                entity.HasIndex(v => new { v.EntityType, v.EntityId });

                // Properties
                entity.Property(v => v.CustomFieldId).IsRequired().HasMaxLength(32);
                entity.Property(v => v.EntityId).IsRequired().HasMaxLength(32);
                entity.Property(v => v.FieldName).IsRequired().HasMaxLength(255);
                entity.Property(v => v.FieldValue).IsRequired();

                // Relationship: CustomFieldValue -> CustomField
                entity.HasOne(v => v.CustomField)
                    .WithMany(f => f.CustomFieldValues)
                    .HasForeignKey(v => v.CustomFieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MembershipGroup 1-n CustomFieldValue (for GroupMembership entity type)
            modelBuilder.Entity<MembershipGroup>(entity =>
            {
                // Navigation property for custom field values
                entity.HasMany(mg => mg.CustomFieldValues)
                    .WithOne()
                    .HasForeignKey(cfv => cfv.EntityId)
                    .HasPrincipalKey(mg => mg.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for tracking custom field submission status
                entity.HasIndex(mg => mg.HasCustomFieldsSubmitted);
            });
        }
    }
}


