using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;

namespace MiniAppGIBA.Services.Logs
{
    public interface IReferralLogService
    {
        /// <summary>
        /// Tạo referral log (automatic logging khi có giới thiệu)
        /// </summary>
        Task<ReferralLog> LogReferralAsync(CreateReferralLogDto dto);

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        Task<PagedResult<ReferralLogDto>> GetReferralLogsAsync(ReferralLogQueryParameters queryParameters);

        /// <summary>
        /// Lấy logs theo nhóm (cho ADMIN chỉ xem nhóm mình quản lý)
        /// </summary>
        Task<PagedResult<ReferralLogDto>> GetLogsByGroupsAsync(List<string> groupIds, ReferralLogQueryParameters queryParameters);

        /// <summary>
        /// Thống kê top referrers
        /// </summary>
        Task<Dictionary<string, int>> GetTopReferrersAsync(List<string>? groupIds = null, int top = 10);
    }
}

