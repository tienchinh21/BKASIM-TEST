using MiniAppGIBA.Models.DTOs.Subscriptions;

namespace MiniAppGIBA.Services.Subscriptions
{
    /// <summary>
    /// Service quản lý subscription của user
    /// </summary>
    public interface ISubscriptionManagementService
    {
        /// <summary>
        /// Lấy danh sách subscription của user theo UserZaloId
        /// </summary>
        Task<List<UserSubscriptionDTO>> GetUserSubscriptionsByUserZaloIdAsync(string userZaloId);

        /// <summary>
        /// Thêm subscription mới cho user
        /// </summary>
        Task<MemberSubscriptionDTO> AddUserSubscriptionAsync(string userZaloId, string subscriptionPlanId, DateTime? startDate, int? additionalDays, string? notes);

        /// <summary>
        /// Cập nhật/gia hạn subscription
        /// </summary>
        Task UpdateUserSubscriptionAsync(string subscriptionId, int? additionalDays, string? notes, bool? isActive);
    }

    /// <summary>
    /// DTO cho subscription của user với đầy đủ thông tin
    /// </summary>
    public class UserSubscriptionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string MembershipGroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal? Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

