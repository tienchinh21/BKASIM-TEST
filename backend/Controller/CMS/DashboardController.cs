using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Services.Dashboard;
using MiniAppGIBA.Constants;

namespace MiniAppGIBA.Controller.CMS
{
    /// <summary>
    /// Dashboard Controller - Gom toàn bộ logic thống kê dashboard ở đây
    /// Không cần tách API riêng, CMS tự handle
    /// </summary>
    // [Authorize(Roles = CTRole.GIBA)]
    [Authorize]
    public class DashboardController : BaseCMSController
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Trang Dashboard chính
        /// </summary>
        public IActionResult Index()
        {
            Console.WriteLine($"Dashboard access - IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User name: {User.Identity?.Name}");

            // Lấy token từ TempData và truyền cho view để lưu vào localStorage
            var token = TempData["access_token"]?.ToString();
            if (!string.IsNullOrEmpty(token))
            {
                ViewBag.AccessToken = token;
            }

            return View();
        }

        #region API Endpoints for AJAX Calls

        /// <summary>
        /// API: Lấy thống kê tổng quan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _dashboardService.GetDashboardStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê hội nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupsStatistics()
        {
            try
            {
                var stats = await _dashboardService.GetGroupsStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê hội nhóm" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê sự kiện
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEventsStatistics()
        {
            try
            {
                var stats = await _dashboardService.GetEventsStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê sự kiện" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê thành viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMembersStatistics()
        {
            try
            {
                var stats = await _dashboardService.GetMembersStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê thành viên" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê refs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRefsStatistics()
        {
            try
            {
                var stats = await _dashboardService.GetRefsStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê refs" });
            }
        }

        /// <summary>
        /// API: Lấy danh sách sự kiện sắp tới
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int limit = 5)
        {
            try
            {
                var events = await _dashboardService.GetUpcomingEventsAsync(limit);
                return Json(new { success = true, data = events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming events");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách sự kiện sắp tới" });
            }
        }

        /// <summary>
        /// API: Lấy danh sách hội nhóm mới nhất
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentGroups([FromQuery] int limit = 5)
        {
            try
            {
                var groups = await _dashboardService.GetRecentGroupsAsync(limit);
                return Json(new { success = true, data = groups });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent groups");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách hội nhóm mới nhất" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê theo tháng (12 tháng gần nhất)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthlyStatistics()
        {
            try
            {
                var stats = await _dashboardService.GetMonthlyStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê theo tháng" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê hoạt động gần đây
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
        {
            try
            {
                var activities = await _dashboardService.GetRecentActivitiesAsync(limit);
                return Json(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy hoạt động gần đây" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê Refs theo ngày (30 ngày gần nhất)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRefsDailyStats([FromQuery] int days = 30)
        {
            try
            {
                var stats = await _dashboardService.GetRefsDailyStatsAsync(days);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs daily stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê refs theo ngày" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê Đăng ký sự kiện theo trạng thái
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRegistrationsByStatus()
        {
            try
            {
                var stats = await _dashboardService.GetRegistrationsByStatusAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations by status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê đăng ký theo trạng thái" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê Gói cước
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubscriptionPlansStats()
        {
            try
            {
                var stats = await _dashboardService.GetSubscriptionPlansStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription plans stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê gói cước" });
            }
        }

        /// <summary>
        /// API: Lấy thống kê Thành viên mới theo ngày (7 ngày gần nhất)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNewMembersDailyStats([FromQuery] int days = 7)
        {
            try
            {
                var stats = await _dashboardService.GetNewMembersDailyStatsAsync(days);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new members daily stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê thành viên mới theo ngày" });
            }
        }

        /// <summary>
        /// API: Lấy User Ref Dashboard - Thống kê chi tiết refs của 1 user
        /// Route: /api/dashboard/user-ref-dashboard/{userZaloId}
        /// </summary>
        [HttpGet("/api/dashboard/user-ref-dashboard/{userZaloId}")]
        public async Task<IActionResult> GetUserRefDashboard(string userZaloId)
        {
            try
            {
                var dashboard = await _dashboardService.GetUserRefDashboardAsync(userZaloId);
                return Json(new { success = true, data = dashboard });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserZaloId}", userZaloId);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ref dashboard for {UserZaloId}", userZaloId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy dashboard ref của user" });
            }
        }

        #endregion
    }
}
