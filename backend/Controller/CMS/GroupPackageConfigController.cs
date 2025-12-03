using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using MiniAppGIBA.Services.Subscriptions;

namespace MiniAppGIBA.Controller.CMS
{
    [Route("GroupPackageConfig")]
    public class GroupPackageConfigController : BaseCMSController
    {
        private readonly IGroupPackageConfigService _groupPackageConfigService;
        private readonly ILogger<GroupPackageConfigController> _logger;

        public GroupPackageConfigController(
            IGroupPackageConfigService groupPackageConfigService,
            ILogger<GroupPackageConfigController> logger)
        {
            _groupPackageConfigService = groupPackageConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Trang quản lý gán gói cước vào nhóm
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View("~/Views/GroupPackageConfig/Index.cshtml");
        }

        /// <summary>
        /// API lấy danh sách config với phân trang
        /// </summary>
        [HttpGet("GetPaged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string keyword = "")
        {
            try
            {
                var result = await _groupPackageConfigService.GetPagedAsync(page, pageSize, keyword);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged group package configs");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách" });
            }
        }

        /// <summary>
        /// API tạo mới config
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateGroupPackageConfigDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var result = await _groupPackageConfigService.CreateAsync(request);
                return Json(new { success = true, data = result, message = "Gán gói cước vào nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group package config");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API xóa config
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                var result = await _groupPackageConfigService.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy config" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group package config");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa" });
            }
        }

        /// <summary>
        /// API toggle active
        /// </summary>
        [HttpPut("ToggleActive")]
        public async Task<IActionResult> ToggleActive([FromQuery] string id)
        {
            try
            {
                var result = await _groupPackageConfigService.ToggleActiveAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy config" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật" });
            }
        }

        /// <summary>
        /// API lấy config theo ID
        /// </summary>
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery] string id)
        {
            try
            {
                var result = await _groupPackageConfigService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy config" });
                }
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group package config by id");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy dữ liệu" });
            }
        }

        /// <summary>
        /// API cập nhật config
        /// </summary>
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateGroupPackageConfigDTO request, [FromQuery] string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var result = await _groupPackageConfigService.UpdateAsync(id, request);
                if (result == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy config" });
                }
                return Json(new { success = true, data = result, message = "Cập nhật gói cước thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group package config");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

