namespace MiniAppGIBA.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO thống kê tổng quan Dashboard - Đầy đủ tất cả metrics
    /// </summary>
    public class DashboardStatisticsDTO
    {
        // === GROUPS ===
        public int TotalGroups { get; set; }
        public int ActiveGroups { get; set; }
        public int InactiveGroups { get; set; }

        // === EVENTS ===
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int OngoingEvents { get; set; }
        public int CompletedEvents { get; set; }

        // === MEMBERS ===
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }

        // === PENDING APPROVALS ===
        /// <summary>
        /// Số đơn chờ duyệt vào nhóm (MembershipGroup.IsApproved = null)
        /// </summary>
        public int PendingGroupApprovals { get; set; }

        /// <summary>
        /// Số đơn chờ duyệt khách mời sự kiện (EventGuest.Status = 1)
        /// </summary>
        public int PendingEventGuestApprovals { get; set; }

        /// <summary>
        /// Số đơn đăng ký sự kiện chờ xác nhận (EventRegistration.Status = 1)
        /// </summary>
        public int PendingEventRegistrations { get; set; }

        // === REFS ===
        public int TotalRefs { get; set; }
        public int PendingRefs { get; set; }
        public int CompletedRefs { get; set; }

        // === EVENT REGISTRATIONS ===
        public int TotalEventRegistrations { get; set; }
        public int TotalEventGuests { get; set; }
        public int TotalGuestLists { get; set; }

        // === SUBSCRIPTIONS ===
        /// <summary>
        /// Tổng số gói cước
        /// </summary>
        public int TotalSubscriptionPlans { get; set; }

        /// <summary>
        /// Số gói cước đang active
        /// </summary>
        public int ActiveSubscriptionPlans { get; set; }

        /// <summary>
        /// Tổng số subscription của members
        /// </summary>
        public int TotalMemberSubscriptions { get; set; }

        /// <summary>
        /// Số subscription đang active (chưa hết hạn)
        /// </summary>
        public int ActiveMemberSubscriptions { get; set; }

        /// <summary>
        /// Số subscription sắp hết hạn (trong 7 ngày tới)
        /// </summary>
        public int ExpiringSoonSubscriptions { get; set; }

        // === SPONSORS ===
        public int TotalSponsors { get; set; }
        public int TotalSponsorshipTiers { get; set; }

        // === GIFTS ===
        public int TotalEventGifts { get; set; }
    }

    public class GroupsStatisticsDTO
    {
        public int TotalGroups { get; set; }
        public int ActiveGroups { get; set; }
        public int InactiveGroups { get; set; }
        public int GroupsCreatedThisMonth { get; set; }
        public int GroupsCreatedLastMonth { get; set; }
        public double GroupsGrowthRate { get; set; }
    }

    public class EventsStatisticsDTO
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int OngoingEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int CancelledEvents { get; set; }
        public int EventsThisMonth { get; set; }
        public int EventsLastMonth { get; set; }
        public double EventsGrowthRate { get; set; }
        public int TotalParticipants { get; set; }
        public int AverageParticipantsPerEvent { get; set; }
    }

    public class MembersStatisticsDTO
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int InactiveMembers { get; set; }
        public int NewMembersThisMonth { get; set; }
        public int NewMembersLastMonth { get; set; }
        public double MembersGrowthRate { get; set; }
        public int MembersWithProfile { get; set; }
        public int MembersWithoutProfile { get; set; }
    }

    public class RefsStatisticsDTO
    {
        public int TotalRefs { get; set; }
        public int PendingRefs { get; set; }
        public int CompletedRefs { get; set; }
        public int CancelledRefs { get; set; }
        public int RefsThisMonth { get; set; }
        public int RefsLastMonth { get; set; }
        public double RefsGrowthRate { get; set; }
        public decimal TotalRefValue { get; set; }
        public decimal AverageRefValue { get; set; }
    }

    public class UpcomingEventDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Banner { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int RegisteredCount { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsUnlimited { get; set; }
    }

    public class RecentGroupDTO
    {
        public string Id { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int MemberCount { get; set; }
        public int EventCount { get; set; }
    }

    public class MonthlyStatisticsDTO
    {
        public List<MonthlyDataDTO> GroupsData { get; set; } = new();
        public List<MonthlyDataDTO> EventsData { get; set; } = new();
        public List<MonthlyDataDTO> MembersData { get; set; } = new();
        public List<MonthlyDataDTO> RefsData { get; set; } = new();
    }

    public class MonthlyDataDTO
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Year { get; set; }
    }

    public class RecentActivityDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "group", "event", "member", "ref"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho thống kê theo ngày
    /// </summary>
    public class DailyStatsDTO
    {
        public string Date { get; set; } = string.Empty; // Format: dd/MM
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê đăng ký theo trạng thái
    /// </summary>
    public class RegistrationsByStatusDTO
    {
        public int Confirmed { get; set; } // Status = 2 - Confirmed
        public int Pending { get; set; } // Status = 1 - Registered
        public int Cancelled { get; set; } // Status = 3 - Cancelled
    }

    /// <summary>
    /// DTO cho thống kê gói cước
    /// </summary>
    public class SubscriptionPlanStatsDTO
    {
        public string PlanName { get; set; } = string.Empty;
        public int ActiveCount { get; set; } // Số lượng đang active
        public int TotalCount { get; set; } // Tổng số đã được gán
    }
}
