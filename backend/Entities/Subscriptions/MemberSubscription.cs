using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Subscriptions
{
    /// <summary>
    /// Bảng Theo Dõi Gói Cước Của Thành Viên
    /// </summary>
    public class MemberSubscription : BaseEntity
    {
        public string MembershipGroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } // Ngày bắt đầu
        public DateTime EndDate { get; set; } // Ngày hết hạn
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; } // Ghi chú
    }
}
