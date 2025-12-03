using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;

namespace MiniAppGIBA.Services.Logs
{
    public interface IProfileShareLogService
    {
        /// <summary>
        /// Tạo profile share log (automatic logging khi chia sẻ profile)
        /// </summary>
        Task<ProfileShareLog> LogProfileShareAsync(CreateProfileShareLogDto dto);

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        Task<PagedResult<ProfileShareLogDto>> GetProfileShareLogsAsync(ProfileShareLogQueryParameters queryParameters);

        /// <summary>
        /// Lấy logs theo nhóm (cho ADMIN chỉ xem nhóm mình quản lý)
        /// </summary>
        Task<PagedResult<ProfileShareLogDto>> GetLogsByGroupsAsync(List<string> groupIds, ProfileShareLogQueryParameters queryParameters);

        /// <summary>
        /// Thống kê top sharers
        /// </summary>
        Task<Dictionary<string, int>> GetTopSharersAsync(List<string>? groupIds = null, int top = 10);
    }
}

