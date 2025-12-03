using MiniAppGIBA.Models.DTOs.Subscriptions;

namespace MiniAppGIBA.Services.Subscriptions
{
    public interface IMemberSubscriptionService
    {
        /// <summary>
        /// Lấy danh sách gói cước của thành viên
        /// </summary>
        Task<List<MemberSubscriptionDTO>> GetByMembershipGroupIdAsync(string membershipGroupId);

        /// <summary>
        /// Lấy danh sách gói cước đang hoạt động của thành viên
        /// </summary>
        Task<List<MemberSubscriptionDTO>> GetActiveByMembershipGroupIdAsync(string membershipGroupId);

        /// <summary>
        /// Lấy gói cước hiện tại của thành viên
        /// </summary>
        Task<MemberSubscriptionDTO?> GetCurrentSubscriptionAsync(string membershipGroupId);

        /// <summary>
        /// Tạo gói cước cho thành viên
        /// </summary>
        Task<MemberSubscriptionDTO> CreateAsync(CreateMemberSubscriptionDTO request);

        /// <summary>
        /// Gia hạn gói cước cho thành viên
        /// </summary>
        Task<MemberSubscriptionDTO> ExtendSubscriptionAsync(string membershipGroupId, string planId, int additionalDays, DateTime? customStartDate = null);

        /// <summary>
        /// Kích hoạt/vô hiệu hóa gói cước
        /// </summary>
        Task<bool> ToggleActiveAsync(string id);

        /// <summary>
        /// Kiểm tra gói cước có hết hạn không
        /// </summary>
        Task<bool> IsExpiredAsync(string membershipGroupId);

        /// <summary>
        /// Lấy danh sách gói cước sắp hết hạn
        /// </summary>
        Task<List<MemberSubscriptionDTO>> GetExpiringSoonAsync(int daysBeforeExpiry = 7);
    }
}
