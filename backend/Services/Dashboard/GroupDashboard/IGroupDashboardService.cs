using MiniAppGIBA.Models.DTOs.Dashboard;

namespace MiniAppGIBA.Services.Dashboard.GroupDashboard
{
    public interface IGroupDashboardService
    {
        /// <summary>
        /// Lấy leaderboard của một nhóm cụ thể
        /// </summary>
        Task<List<UserRefListDTO>> GetGroupLeaderboardAsync(string groupId, string period = "month", int limit = 20, string sortBy = "TotalRefs");

        /// <summary>
        /// Lấy thống kê tổng quan của nhóm
        /// </summary>
        Task<GroupDashboardSummaryDTO> GetGroupDashboardSummaryAsync(string groupId);

        /// <summary>
        /// Lấy danh sách nhóm với thống kê leaderboard
        /// </summary>
        Task<List<GroupLeaderboardSummaryDTO>> GetGroupsLeaderboardSummaryAsync(string period = "month");

        /// <summary>
        /// Lấy top 3 thành viên của nhóm
        /// </summary>
        Task<List<UserRefListDTO>> GetGroupTop3Async(string groupId, string period = "month");

        /// <summary>
        /// Lấy thống kê ref theo tháng của nhóm
        /// </summary>
        Task<List<GroupMonthlyRefDataDTO>> GetGroupMonthlyRefDataAsync(string groupId, int months = 12);
    }
}
