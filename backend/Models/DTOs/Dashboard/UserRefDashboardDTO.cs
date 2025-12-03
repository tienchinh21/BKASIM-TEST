namespace MiniAppGIBA.Models.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard thống kê ref của một user cụ thể
    /// </summary>
    public class UserRefDashboardDTO
    {
        public string UserZaloId { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public DateTime CreatedDate { get; set; }

        // Thống kê tổng quan
        public UserRefSummaryDTO Summary { get; set; } = new();

        // Danh sách ref đã trao
        public List<UserRefGivenDTO> RefsGiven { get; set; } = new();

        // Danh sách ref đã nhận
        public List<UserRefReceivedDTO> RefsReceived { get; set; } = new();

        // Thống kê theo tháng (12 tháng gần nhất)
        public List<UserMonthlyRefDataDTO> MonthlyData { get; set; } = new();
    }

    /// <summary>
    /// Tóm tắt thống kê ref của user
    /// </summary>
    public class UserRefSummaryDTO
    {
        // Thống kê trao ref
        public int TotalRefsGiven { get; set; }        // Tổng số ref đã trao
        public int CompletedRefsGiven { get; set; }    // Số ref đã hoàn thành
        public int PendingRefsGiven { get; set; }      // Số ref đang chờ
        public int CancelledRefsGiven { get; set; }    // Số ref đã hủy
        public decimal TotalValueGiven { get; set; }   // Tổng giá trị đã trao
        public decimal AverageValueGiven { get; set; } // Giá trị trung bình

        // Thống kê nhận ref
        public int TotalRefsReceived { get; set; }     // Tổng số ref đã nhận
        public int CompletedRefsReceived { get; set; } // Số ref đã hoàn thành
        public int PendingRefsReceived { get; set; }   // Số ref đang chờ
        public int CancelledRefsReceived { get; set; } // Số ref đã hủy
        public decimal TotalValueReceived { get; set; } // Tổng giá trị đã nhận
        public decimal AverageValueReceived { get; set; } // Giá trị trung bình nhận

        // Tỷ lệ thành công
        public double SuccessRateGiven { get; set; }   // Tỷ lệ thành công khi trao ref
        public double SuccessRateReceived { get; set; } // Tỷ lệ thành công khi nhận ref

        // Thời gian
        public DateTime? LastRefGivenDate { get; set; }    // Lần trao ref cuối
        public DateTime? LastRefReceivedDate { get; set; } // Lần nhận ref cuối
        public int DaysSinceLastRef { get; set; }          // Số ngày từ lần ref cuối
    }

    /// <summary>
    /// Chi tiết ref đã trao của user
    /// </summary>
    public class UserRefGivenDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string RefToUserZaloId { get; set; } = string.Empty;
        public string RefToName { get; set; } = string.Empty;
        public string RefToPhone { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Cancelled"
        public string StatusText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Chi tiết ref đã nhận của user
    /// </summary>
    public class UserRefReceivedDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string RefFromUserZaloId { get; set; } = string.Empty;
        public string RefFromName { get; set; } = string.Empty;
        public string RefFromPhone { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Cancelled"
        public string StatusText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Thống kê ref theo tháng của user
    /// </summary>
    public class UserMonthlyRefDataDTO
    {
        public string Month { get; set; } = string.Empty; // "Jan 2024", "Feb 2024", etc.
        public int Year { get; set; }
        public int MonthNumber { get; set; }

        // Ref đã trao trong tháng
        public int RefsGiven { get; set; }
        public int CompletedRefsGiven { get; set; }
        public decimal ValueGiven { get; set; }

        // Ref đã nhận trong tháng
        public int RefsReceived { get; set; }
        public int CompletedRefsReceived { get; set; }
        public decimal ValueReceived { get; set; }

        // Tổng kết
        public int TotalRefs { get; set; }
        public decimal TotalValue { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// DTO cho danh sách user với thống kê ref (dùng cho admin)
    /// </summary>
    public class UserRefListDTO
    {
        public string UserZaloId { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? Company { get; set; }
        public DateTime CreatedDate { get; set; }

        // Thống kê tóm tắt
        public int TotalRefsGiven { get; set; }
        public int TotalRefsReceived { get; set; }
        public decimal TotalValueGiven { get; set; }
        public decimal TotalValueReceived { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastRefDate { get; set; }

        // Xếp hạng
        public int RefRanking { get; set; }
        public int ValueRanking { get; set; }
    }

    /// <summary>
    /// DTO cho bộ lọc và phân trang user ref dashboard
    /// </summary>
    public class UserRefDashboardQueryDTO
    {
        public string? UserZaloId { get; set; }
        public string? SearchKeyword { get; set; }
        public string? SortBy { get; set; } = "LastRefDate"; // "LastRefDate", "TotalRefs", "TotalValue", "SuccessRate"
        public string? SortDirection { get; set; } = "desc"; // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; } // "All", "Active", "Inactive"
    }

    /// <summary>
    /// DTO cho kết quả phân trang user ref dashboard
    /// </summary>
    public class UserRefDashboardResultDTO
    {
        public List<UserRefListDTO> Users { get; set; } = new();
        public int TotalUsers { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        // Thống kê tổng quan của tất cả users
        public UserRefSummaryDTO OverallSummary { get; set; } = new();
    }
}
