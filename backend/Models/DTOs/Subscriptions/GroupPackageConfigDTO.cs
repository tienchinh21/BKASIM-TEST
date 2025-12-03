namespace MiniAppGIBA.Models.DTOs.Subscriptions
{
    /// <summary>
    /// DTO cho GroupPackageConfig
    /// </summary>
    public class GroupPackageConfigDTO
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// DTO cho tạo mới GroupPackageConfig
    /// </summary>
    public class CreateGroupPackageConfigDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO cho cập nhật GroupPackageConfig
    /// </summary>
    public class UpdateGroupPackageConfigDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
