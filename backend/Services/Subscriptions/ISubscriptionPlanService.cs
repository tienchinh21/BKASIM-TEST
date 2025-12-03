using MiniAppGIBA.Models.DTOs.Subscriptions;
using MiniAppGIBA.Models.Requests.Common;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Services.Subscriptions
{
    public interface ISubscriptionPlanService
    {
        /// <summary>
        /// Lấy danh sách gói cước với phân trang
        /// </summary>
        Task<PagedResult<SubscriptionPlanDTO>> GetPagedAsync(BaseQueryParameters queryParams);

        /// <summary>
        /// Lấy danh sách gói cước đang hoạt động
        /// </summary>
        Task<List<SubscriptionPlanDTO>> GetActivePlansAsync();

        /// <summary>
        /// Lấy danh sách gói cước theo GroupId (từ GroupPackageConfig)
        /// </summary>
        Task<List<SubscriptionPlanDTO>> GetPlansByGroupIdAsync(string groupId);

        /// <summary>
        /// Tạo mới gói cước
        /// </summary>
        Task<SubscriptionPlanDTO> CreateAsync(CreateSubscriptionPlanDTO request);

        /// <summary>
        /// Cập nhật gói cước
        /// </summary>
        Task<SubscriptionPlanDTO> UpdateAsync(UpdateSubscriptionPlanDTO request);

        /// <summary>
        /// Xóa gói cước (soft delete)
        /// </summary>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Kích hoạt/vô hiệu hóa gói cước
        /// </summary>
        Task<bool> ToggleActiveAsync(string id);
    }
}
