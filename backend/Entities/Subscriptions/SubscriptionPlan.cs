using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Subscriptions
{
    /// <summary>
    /// Bảng Gói Cước/Subscription Plan
    /// </summary>
    public class SubscriptionPlan : BaseEntity
    {
        public string PlanName { get; set; } = string.Empty; // Tên gói
        public string? Description { get; set; } // Mô tả
        public int DurationDays { get; set; } // Số ngày có hiệu lực
        public decimal? Price { get; set; } // Giá (nếu có)
        public bool IsActive { get; set; } = true;
        public string? Features { get; set; } // Tính năng (JSON)
    }
}
