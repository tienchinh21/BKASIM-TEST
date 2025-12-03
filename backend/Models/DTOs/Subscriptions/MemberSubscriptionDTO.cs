namespace MiniAppGIBA.Models.DTOs.Subscriptions
{
    /// <summary>
    /// DTO cho MemberSubscription
    /// </summary>
    public class MemberSubscriptionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string MembershipGroupId { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// DTO cho tạo mới MemberSubscription
    /// </summary>
    public class CreateMemberSubscriptionDTO
    {
        public string MembershipGroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
