namespace MiniAppGIBA.Models.DTOs.Subscriptions
{
    /// <summary>
    /// DTO cho SubscriptionPlan
    /// </summary>
    public class SubscriptionPlanDTO
    {
        public string Id { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationDays { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public string? Features { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// DTO cho tạo mới SubscriptionPlan
    /// </summary>
    public class CreateSubscriptionPlanDTO
    {
        public string PlanName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationDays { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Features { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật SubscriptionPlan
    /// </summary>
    public class UpdateSubscriptionPlanDTO
    {
        public string Id { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationDays { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public string? Features { get; set; }
    }
}
