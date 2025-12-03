using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Services.Subscriptions;
using MiniAppGIBA.Exceptions;

namespace MiniAppGIBA.Controller.CMS
{
    /// <summary>
    /// Controller quản lý gói cước của thành viên (chỉ handle request/response, logic ở service)
    /// </summary>
    [Authorize]
    [Route("Membership")]
    public class SubscriptionManagementController : BaseCMSController
    {
        private readonly ISubscriptionManagementService _subscriptionManagementService;
        private readonly ILogger<SubscriptionManagementController> _logger;

        public SubscriptionManagementController(
            ISubscriptionManagementService subscriptionManagementService,
            ILogger<SubscriptionManagementController> logger)
        {
            _subscriptionManagementService = subscriptionManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách subscription của user theo UserZaloId
        /// </summary>
        [HttpGet("GetUserSubscriptions/{userZaloId}")]
        public async Task<IActionResult> GetUserSubscriptions(string userZaloId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var subscriptions = await _subscriptionManagementService.GetUserSubscriptionsByUserZaloIdAsync(userZaloId);

                return Json(new { success = true, data = subscriptions });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning(ex, "Failed to get user subscriptions for UserZaloId: {UserZaloId}", userZaloId);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user subscriptions for UserZaloId: {UserZaloId}", userZaloId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// Thêm subscription mới cho user
        /// </summary>
        [HttpPost("AddUserSubscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserSubscription([FromBody] AddUserSubscriptionRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var subscription = await _subscriptionManagementService.AddUserSubscriptionAsync(
                    request.UserZaloId,
                    request.SubscriptionPlanId,
                    request.StartDate,
                    request.AdditionalDays,
                    request.Notes
                );

                return Json(new { success = true, message = "Thêm gói cước thành công!", data = subscription });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning(ex, "Failed to add user subscription");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user subscription");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm gói cước" });
            }
        }

        /// <summary>
        /// Cập nhật/gia hạn subscription
        /// </summary>
        [HttpPost("UpdateUserSubscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserSubscription([FromBody] UpdateUserSubscriptionRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                await _subscriptionManagementService.UpdateUserSubscriptionAsync(
                    request.SubscriptionId,
                    request.AdditionalDays,
                    request.Notes,
                    request.IsActive
                );

                return Json(new { success = true, message = "Cập nhật gói cước thành công!" });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning(ex, "Failed to update user subscription");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user subscription");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật gói cước" });
            }
        }
    }

    /// <summary>
    /// Request model for adding user subscription
    /// </summary>
    public class AddUserSubscriptionRequest
    {
        public string UserZaloId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public int? AdditionalDays { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request model for updating user subscription
    /// </summary>
    public class UpdateUserSubscriptionRequest
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public int? AdditionalDays { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }
}

