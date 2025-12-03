using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;

namespace MiniAppGIBA.Services.Logs
{
    public interface IActivityLogService
    {
        /// <summary>
        /// Tạo activity log (automatic logging)
        /// </summary>
        Task<ActivityLog> LogActivityAsync(CreateActivityLogDto dto);

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQueryParameters queryParameters);

        /// <summary>
        /// Lấy logs của ADMIN (chỉ logs của chính mình)
        /// </summary>
        Task<PagedResult<ActivityLogDto>> GetLogsByAdminAsync(string adminId, ActivityLogQueryParameters queryParameters);

        /// <summary>
        /// Lấy chi tiết activity log theo ID
        /// </summary>
        Task<ActivityLogDto?> GetActivityLogByIdAsync(string id);

        /// <summary>
        /// Export activity logs to Excel
        /// </summary>
        Task<byte[]> ExportToExcelAsync(IEnumerable<ActivityLogDto> logs);

        /// <summary>
        /// Thống kê logs theo ActionType
        /// </summary>
        Task<Dictionary<string, int>> GetStatisticsByActionTypeAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

