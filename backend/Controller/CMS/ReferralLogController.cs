using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Logs;
using MiniAppGIBA.Services.Admins;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Constants;
namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(Roles = CTRole.GIBA)]
    public class ReferralLogController : BaseCMSController
    {
        private readonly IReferralLogService _referralLogService;
        private readonly IGroupPermissionService _groupPermissionService;
        private readonly ILogger<ReferralLogController> _logger;

        public ReferralLogController(
            IReferralLogService referralLogService,
            IGroupPermissionService groupPermissionService,
            ILogger<ReferralLogController> logger)
        {
            _referralLogService = referralLogService;
            _groupPermissionService = groupPermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách referral logs
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
                _logger.LogError(ex, "Error loading referral logs page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang referral logs");
                return View();
            }
        }

        /// <summary>
        /// API endpoint cho DataTable - lấy referral logs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPage([FromQuery] ReferralLogQueryParameters query)
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
                    ? await _referralLogService.GetReferralLogsAsync(query)
                    : await _referralLogService.GetLogsByGroupsAsync(
                        await _groupPermissionService.GetGroupIdsByUserIdAsync(currentUserId),
                        query);

                var result = new
                {
                    data = logs.Items.Select(l => new
                    {
                        id = l.Id,
                        referrerName = l.ReferrerName ?? "Unknown",
                        refereeName = l.RefereeName ?? "Unknown",
                        groupName = l.GroupName ?? "Unknown",
                        referralCode = l.ReferralCode ?? "",
                        source = l.Source ?? "",
                        createdDate = l.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss")
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
                _logger.LogError(ex, "Error getting referral logs page");
                return Json(new { data = new List<object>(), totalItems = 0 });
            }
        }

        /// <summary>
        /// API lấy thống kê top referrers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopReferrers(int top = 10)
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

                var statistics = await _referralLogService.GetTopReferrersAsync(groupIds, top);
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top referrers");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Export referral logs to CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] ReferralLogQueryParameters query)
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
                    ? await _referralLogService.GetReferralLogsAsync(query)
                    : await _referralLogService.GetLogsByGroupsAsync(
                        await _groupPermissionService.GetGroupIdsByUserIdAsync(currentUserId),
                        query);

                var csv = "Referrer,Referee,Group,Referral Code,Source,Date\n";
                foreach (var log in logs.Items)
                {
                    csv += $"\"{log.ReferrerName}\",\"{log.RefereeName}\",\"{log.GroupName}\",\"{log.ReferralCode}\",\"{log.Source}\",\"{log.CreatedDate:dd/MM/yyyy HH:mm:ss}\"\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"ReferralLogs_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting referral logs");
                SetErrorMessage("Có lỗi xảy ra khi export logs");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

