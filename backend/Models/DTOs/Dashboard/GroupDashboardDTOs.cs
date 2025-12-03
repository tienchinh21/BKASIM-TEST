namespace MiniAppGIBA.Models.DTOs.Dashboard
{
    /// <summary>
    /// Tóm tắt thống kê dashboard của một nhóm
    /// </summary>
    public class GroupDashboardSummaryDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }

        // Thống kê ref của nhóm
        public int TotalRefsGiven { get; set; }
        public int CompletedRefsGiven { get; set; }
        public decimal TotalValueGiven { get; set; }
        public double SuccessRate { get; set; }

        // Thống kê theo thời gian
        public int RefsThisMonth { get; set; }
        public int RefsLastMonth { get; set; }
        public double GrowthRate { get; set; }

        // Top performer
        public string? TopPerformerName { get; set; }
        public int TopPerformerRefs { get; set; }
    }

    /// <summary>
    /// Tóm tắt leaderboard của tất cả nhóm
    /// </summary>
    public class GroupLeaderboardSummaryDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
        public int TotalRefs { get; set; }
        public decimal TotalValue { get; set; }
        public double SuccessRate { get; set; }
        public int Ranking { get; set; }
        public string? TopPerformerName { get; set; }
        public int TopPerformerRefs { get; set; }
    }

    /// <summary>
    /// Thống kê ref theo tháng của nhóm
    /// </summary>
    public class GroupMonthlyRefDataDTO
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int MonthNumber { get; set; }

        // Ref của nhóm trong tháng
        public int TotalRefs { get; set; }
        public int CompletedRefs { get; set; }
        public decimal TotalValue { get; set; }
        public double SuccessRate { get; set; }
        public int RefsReceived { get; set; }

        // Số thành viên hoạt động
        public int ActiveMembers { get; set; }
        public int NewMembers { get; set; }
    }

    /// <summary>
    /// Query parameters cho group dashboard
    /// </summary>
    public class GroupDashboardQueryDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string Period { get; set; } = "month"; // week, month, year, all
        public int Limit { get; set; } = 20;
        public string SortBy { get; set; } = "TotalRefs"; // TotalRefs, TotalValue, SuccessRate
        public string SortDirection { get; set; } = "desc"; // asc, desc
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
