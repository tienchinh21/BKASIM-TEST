using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.CMS;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using MiniAppGIBA.Models.Requests.Common;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Services.Subscriptions;

namespace MiniAppGIBA.Controller.CMS
{
    public class SubscriptionPlanController : BaseCMSController
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly ILogger<SubscriptionPlanController> _logger;

        public SubscriptionPlanController(
            ISubscriptionPlanService subscriptionPlanService,
            ILogger<SubscriptionPlanController> logger)
        {
            _subscriptionPlanService = subscriptionPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách gói cước
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var queryParams = new BaseQueryParameters
                {
                    Page = 1,
                    PageSize = 20,
                    Keyword = ""
                };

                var result = await _subscriptionPlanService.GetPagedAsync(queryParams);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscription plans page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang gói cước");
                return View(new PagedResult<SubscriptionPlanDTO>());
            }
        }

        /// <summary>
        /// API lấy danh sách gói cước với phân trang
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged(BaseQueryParameters queryParams, string? status = null)
        {
            try
            {
                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                {
                    queryParams.Status = bool.Parse(status);
                }

                var result = await _subscriptionPlanService.GetPagedAsync(queryParams);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged subscription plans");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// API lấy danh sách gói cước đang hoạt động
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetActivePlans()
        {
            try
            {
                var plans = await _subscriptionPlanService.GetActivePlansAsync();
                return Json(new { success = true, data = plans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscription plans");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// API lấy tất cả gói cước (không phân trang) - dùng cho dropdown/select
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var queryParams = new BaseQueryParameters
                {
                    Page = 1,
                    PageSize = 1000,
                    Keyword = ""
                };

                var result = await _subscriptionPlanService.GetPagedAsync(queryParams);
                return Json(new { success = true, data = result.Items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subscription plans");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// API tạo mới gói cước
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionPlanDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var result = await _subscriptionPlanService.CreateAsync(request);
                return Json(new { success = true, data = result, message = "Tạo gói cước thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription plan");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo gói cước" });
            }
        }

        /// <summary>
        /// API cập nhật gói cước
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSubscriptionPlanDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var result = await _subscriptionPlanService.UpdateAsync(request);
                return Json(new { success = true, data = result, message = "Cập nhật gói cước thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription plan");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật gói cước" });
            }
        }

        /// <summary>
        /// API xóa gói cước
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _subscriptionPlanService.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa gói cước thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Gói cước không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription plan");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa gói cước" });
            }
        }

        /// <summary>
        /// API kích hoạt/vô hiệu hóa gói cước
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleActive(string id)
        {
            try
            {
                var result = await _subscriptionPlanService.ToggleActiveAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái gói cước thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Gói cước không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling subscription plan active status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái gói cước" });
            }
        }
    }
}
