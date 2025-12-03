using MiniAppGIBA.Models.DTOs.Dashboard;

namespace MiniAppGIBA.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDTO> GetDashboardStatisticsAsync();
        Task<GroupsStatisticsDTO> GetGroupsStatisticsAsync();
        Task<EventsStatisticsDTO> GetEventsStatisticsAsync();
        Task<MembersStatisticsDTO> GetMembersStatisticsAsync();
        Task<RefsStatisticsDTO> GetRefsStatisticsAsync();
        Task<List<UpcomingEventDTO>> GetUpcomingEventsAsync(int limit = 5);
        Task<List<RecentGroupDTO>> GetRecentGroupsAsync(int limit = 5);
        Task<MonthlyStatisticsDTO> GetMonthlyStatisticsAsync();
        Task<List<RecentActivityDTO>> GetRecentActivitiesAsync(int limit = 10);

        // New APIs for Dashboard charts
        Task<List<DailyStatsDTO>> GetRefsDailyStatsAsync(int days = 30);
        Task<RegistrationsByStatusDTO> GetRegistrationsByStatusAsync();
        Task<List<SubscriptionPlanStatsDTO>> GetSubscriptionPlansStatsAsync();
        Task<List<DailyStatsDTO>> GetNewMembersDailyStatsAsync(int days = 7);

        Task<UserRefDashboardResultDTO> GetUserRefsListAsync(UserRefDashboardQueryDTO query);
        Task<UserRefDashboardDTO> GetUserRefDashboardAsync(string userZaloId);
        Task<List<UserRefListDTO>> GetTopReferrersAsync(int limit = 10);
        Task<List<UserMonthlyRefDataDTO>> GetRefTimelineAsync(string period = "month");
        Task<List<UserRefListDTO>> GetRefLeaderboardAsync(string period = "month", int limit = 20);
    }
}
