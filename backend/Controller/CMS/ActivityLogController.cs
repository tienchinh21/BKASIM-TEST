using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Logs;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Constants;
namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(Roles = CTRole.GIBA)]
    public class ActivityLogController : BaseCMSController
    {
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<ActivityLogController> _logger;

        public ActivityLogController(
            IActivityLogService activityLogService,
            ILogger<ActivityLogController> logger)
        {
            _activityLogService = activityLogService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách activity logs
        /// SUPER_ADMIN: xem tất cả
        /// ADMIN: chỉ xem logs của chính mình
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var isSuperAdmin = User.IsInRole(CTRole.GIBA);
                ViewBag.IsSuperAdmin = isSuperAdmin;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity logs page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang activity logs");
                return View();
            }
        }

        /// <summary>
        /// API endpoint cho DataTable - lấy activity logs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPage([FromQuery] ActivityLogQueryParameters query)
        {
            try
            {
                var isSuperAdmin = User.IsInRole(CTRole.GIBA);
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { data = new List<object>(), totalItems = 0 });
                }

                var logs = isSuperAdmin
                    ? await _activityLogService.GetActivityLogsAsync(query)
                    : await _activityLogService.GetLogsByAdminAsync(currentUserId, query);

                var result = new
                {
                    data = logs.Items.Select(l => new
                    {
                        id = l.Id,
                        accountFullName = l.AccountFullName,
                        accountEmail = l.AccountEmail,
                        actionType = l.ActionType,
                        description = l.Description ?? "",
                        targetEntity = l.TargetEntity ?? "",
                        targetId = l.TargetId ?? "",
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
                _logger.LogError(ex, "Error getting activity logs page");
                return Json(new { data = new List<object>(), totalItems = 0 });
            }
        }

        /// <summary>
        /// API lấy thống kê theo ActionType
        /// </summary>
        [HttpGet]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<IActionResult> GetStatistics(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var statistics = await _activityLogService.GetStatisticsByActionTypeAsync(fromDate, toDate);
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity log statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Get activity log detail by ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDetail(string id)
        {
            try
            {
                var log = await _activityLogService.GetActivityLogByIdAsync(id);

                if (log == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy log" });
                }

                // Check permission: ADMIN can only view their own logs
                var isSuperAdmin = User.IsInRole(CTRole.GIBA);
                var currentUserId = GetCurrentUserId();

                if (!isSuperAdmin && log.AccountId != currentUserId)
                {
                    return Json(new { success = false, message = "Không có quyền xem log này" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = log.Id,
                        accountId = log.AccountId,
                        accountFullName = log.AccountFullName,
                        accountEmail = log.AccountEmail,
                        actionType = log.ActionType,
                        description = log.Description ?? "",
                        targetEntity = log.TargetEntity ?? "",
                        targetId = log.TargetId ?? "",
                        metadata = log.Metadata ?? "",
                        createdDate = log.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity log detail");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Export activity logs to Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] ActivityLogQueryParameters query)
        {
            try
            {
                var isSuperAdmin = User.IsInRole(CTRole.GIBA);
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    SetErrorMessage("Không xác định được người dùng");
                    return RedirectToAction(nameof(Index));
                }

                // Get all logs (no pagination for export)
                query.PageSize = int.MaxValue;
                var logs = isSuperAdmin
                    ? await _activityLogService.GetActivityLogsAsync(query)
                    : await _activityLogService.GetLogsByAdminAsync(currentUserId, query);

                var excelData = await _activityLogService.ExportToExcelAsync(logs.Items);

                var fileName = $"LichSuHoatDong_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting activity logs");
                SetErrorMessage("Có lỗi xảy ra khi export logs");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

