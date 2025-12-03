using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Logs;
using MiniAppGIBA.Services.Admins;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Constants;
namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(Roles = CTRole.GIBA)]
    public class ProfileShareLogController : BaseCMSController
    {
        private readonly IProfileShareLogService _profileShareLogService;
        private readonly IGroupPermissionService _groupPermissionService;
        private readonly ILogger<ProfileShareLogController> _logger;

        public ProfileShareLogController(
            IProfileShareLogService profileShareLogService,
            IGroupPermissionService groupPermissionService,
            ILogger<ProfileShareLogController> logger)
        {
            _profileShareLogService = profileShareLogService;
            _groupPermissionService = groupPermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách profile share logs
        /// SUPER_ADMIN: xem tất cả
        /// ADMIN: chỉ xem logs của nhóm mình quản lý
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var isSuperAdmin = User.IsInRole("SUPER_ADMIN");
                ViewBag.IsSuperAdmin = isSuperAdmin;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile share logs page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang profile share logs");
                return View();
            }
        }

        /// <summary>
        /// API endpoint cho DataTable - lấy profile share logs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPage([FromQuery] ProfileShareLogQueryParameters query)
        {
            try
            {
                var isSuperAdmin = User.IsInRole("SUPER_ADMIN");
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { data = new List<object>(), totalItems = 0 });
                }

                var logs = isSuperAdmin
                    ? await _profileShareLogService.GetProfileShareLogsAsync(query)
                    : await _profileShareLogService.GetLogsByGroupsAsync(
                        await _groupPermissionService.GetGroupIdsByUserIdAsync(currentUserId),
                        query);

                var result = new
                {
                    data = logs.Items.Select(l => new
                    {
                        id = l.Id,
                        sharerName = l.SharerName ?? "Unknown",
                        receiverName = l.ReceiverName ?? "Unknown",
                        groupName = l.GroupName ?? "Unknown",
                        shareMethod = l.ShareMethod ?? "",
                        createdDate = l.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    }).ToList(),
                    totalItems = logs.TotalItems,
                    page = logs.Page,
                    pageSize = logs.PageSize,
                    totalPages = logs.TotalPages
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile share logs page");
                return Json(new { data = new List<object>(), totalItems = 0 });
            }
        }

        /// <summary>
        /// API lấy thống kê top sharers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopSharers(int top = 10)
        {
            try
            {
                var isSuperAdmin = User.IsInRole("SUPER_ADMIN");
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "Không xác định được người dùng" });
                }

                var groupIds = isSuperAdmin
                    ? null
                    : await _groupPermissionService.GetGroupIdsByUserIdAsync(currentUserId);

                var statistics = await _profileShareLogService.GetTopSharersAsync(groupIds, top);
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top sharers");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Export profile share logs to CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] ProfileShareLogQueryParameters query)
        {
            try
            {
                var isSuperAdmin = User.IsInRole("SUPER_ADMIN");
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    SetErrorMessage("Không xác định được người dùng");
                    return RedirectToAction(nameof(Index));
                }

                // Get all logs (no pagination for export)
                query.PageSize = int.MaxValue;
                var logs = isSuperAdmin
                    ? await _profileShareLogService.GetProfileShareLogsAsync(query)
                    : await _profileShareLogService.GetLogsByGroupsAsync(
                        await _groupPermissionService.GetGroupIdsByUserIdAsync(currentUserId),
                        query);

                var csv = "Sharer,Receiver,Group,Share Method,Date\n";
                foreach (var log in logs.Items)
                {
                    csv += $"\"{log.SharerName}\",\"{log.ReceiverName}\",\"{log.GroupName}\",\"{log.ShareMethod}\",\"{log.CreatedDate:dd/MM/yyyy HH:mm:ss}\"\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"ProfileShareLogs_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting profile share logs");
                SetErrorMessage("Có lỗi xảy ra khi export logs");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

