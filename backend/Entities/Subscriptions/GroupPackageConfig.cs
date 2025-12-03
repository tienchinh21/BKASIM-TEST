using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Subscriptions
{
    /// <summary>
    /// Bảng Cấu Hình Gói Cho Nhóm
    /// </summary>
    public class GroupPackageConfig : BaseEntity
    {
        public string GroupId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
