using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;
using MiniAppGIBA.Services.CustomFields;

namespace MiniAppGIBA.Controller.CMS
{
    /// <summary>
    /// CMS Controller for managing custom field tabs for groups
    /// Handles CRUD operations for tabs used in membership registration forms
    /// </summary>
    [Authorize]
    public class CustomFieldTabController : BaseCMSController
    {
        private readonly ICustomFieldTabService _customFieldTabService;
        private readonly ILogger<CustomFieldTabController> _logger;

        public CustomFieldTabController(
            ICustomFieldTabService customFieldTabService,
            ILogger<CustomFieldTabController> logger)
        {
            _customFieldTabService = customFieldTabService;
            _logger = logger;
        }

        /// <summary>
        /// Display the group selection page for custom field configuration
        /// </summary>
        [HttpGet]
        public IActionResult SelectGroup()
        {
            try
            {
                // Only GIBA (super admin) can manage custom field tabs
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ GIBA mới có quyền quản lý các tab trường tùy chỉnh!");
                    return RedirectToAction("Index", "Groups");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group selection page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang chọn hội nhóm");
                return RedirectToAction("Index", "Groups");
            }
        }

        /// <summary>
        /// Display the custom field tabs management page for a group
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string groupId)
        {
            try
            {
                // Only GIBA (super admin) can manage custom field tabs
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ GIBA mới có quyền quản lý các tab trường tùy chỉnh!");
                    return RedirectToAction("Index", "Groups");
                }

                if (string.IsNullOrEmpty(groupId) || groupId == "all")
                {
                    return RedirectToAction("SelectGroup");
                }

                ViewBag.GroupId = groupId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading custom field tabs index for group {GroupId}", groupId);
                SetErrorMessage("Có lỗi xảy ra khi tải trang quản lý tab trường tùy chỉnh");
                return RedirectToAction("Index", "Groups");
            }
        }

        /// <summary>
        /// API endpoint to get all tabs for a group
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTabs(string groupId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return Json(new { success = false, message = "ID hội nhóm không được để trống" });
                }

                var tabs = await _customFieldTabService.GetTabsByEntityAsync(
                    ECustomFieldEntityType.GroupMembership,
                    groupId);

                return Json(new
                {
                    success = true,
                    data = tabs.OrderBy(t => t.DisplayOrder).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tabs for group {GroupId}", groupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách tab" });
            }
        }

        /// <summary>
        /// API endpoint to create a new tab for a group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTab([FromBody] CreateCustomFieldTabRequest request)
        {
            try
            {
                // Only GIBA can create tabs
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền tạo tab trường tùy chỉnh!" });
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (string.IsNullOrWhiteSpace(request.TabName))
                {
                    return Json(new { success = false, message = "Tên tab không được để trống" });
                }

                if (string.IsNullOrEmpty(request.EntityId))
                {
                    return Json(new { success = false, message = "ID hội nhóm không được để trống" });
                }

                // Ensure entity type is GroupMembership
                request.EntityType = ECustomFieldEntityType.GroupMembership;

                var createdTab = await _customFieldTabService.CreateTabAsync(request);

                _logger.LogInformation("Tab created successfully for group {GroupId}: {TabName}",
                    request.EntityId, request.TabName);

                return Json(new
                {
                    success = true,
                    message = "Tạo tab thành công",
                    data = createdTab
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating tab");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tab: {Message}", ex.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo tab: " + ex.Message });
            }
        }

        /// <summary>
        /// API endpoint to update an existing tab
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateTab([FromBody] UpdateCustomFieldTabRequest request)
        {
            try
            {
                // Only GIBA can update tabs
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền cập nhật tab trường tùy chỉnh!" });
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (string.IsNullOrEmpty(request.Id))
                {
                    return Json(new { success = false, message = "ID tab không được để trống" });
                }

                if (string.IsNullOrWhiteSpace(request.TabName))
                {
                    return Json(new { success = false, message = "Tên tab không được để trống" });
                }

                var updatedTab = await _customFieldTabService.UpdateTabAsync(request);

                _logger.LogInformation("Tab updated successfully: {TabId}", request.Id);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật tab thành công",
                    data = updatedTab
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tab not found when updating");
                return Json(new { success = false, message = "Không tìm thấy tab" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating tab");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tab");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật tab" });
            }
        }

        /// <summary>
        /// API endpoint to delete a tab and all its associated fields
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteTab(string tabId)
        {
            try
            {
                // Only GIBA can delete tabs
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa tab trường tùy chỉnh!" });
                }

                if (string.IsNullOrEmpty(tabId))
                {
                    return Json(new { success = false, message = "ID tab không được để trống" });
                }

                var result = await _customFieldTabService.DeleteTabAsync(tabId);

                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy tab" });
                }

                _logger.LogInformation("Tab deleted successfully: {TabId}", tabId);

                return Json(new
                {
                    success = true,
                    message = "Xóa tab thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting tab");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tab {TabId}", tabId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tab" });
            }
        }

        /// <summary>
        /// API endpoint to reorder tabs within a group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReorderTabs([FromBody] ReorderTabsRequest request)
        {
            try
            {
                // Only GIBA can reorder tabs
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền sắp xếp lại tab!" });
                }

                if (request == null || string.IsNullOrEmpty(request.EntityId))
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (request.Tabs == null || request.Tabs.Count == 0)
                {
                    return Json(new { success = false, message = "Danh sách tab không được để trống" });
                }

                var reordering = request.Tabs
                    .Select((tab, index) => (TabId: tab.TabId, Order: index + 1))
                    .ToList();

                var result = await _customFieldTabService.ReorderTabsAsync(request.EntityId, reordering);

                if (!result)
                {
                    return Json(new { success = false, message = "Không thể sắp xếp lại tab" });
                }

                _logger.LogInformation("Tabs reordered successfully for group {GroupId}", request.EntityId);

                return Json(new
                {
                    success = true,
                    message = "Sắp xếp lại tab thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when reordering tabs");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering tabs for group {GroupId}", request?.EntityId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi sắp xếp lại tab" });
            }
        }
    }

    /// <summary>
    /// Request model for reordering tabs
    /// </summary>
    public class ReorderTabsRequest
    {
        /// <summary>
        /// The ID of the entity (group) containing the tabs
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// List of tabs with their new order
        /// </summary>
        public List<TabOrderItem> Tabs { get; set; } = new();
    }

    /// <summary>
    /// Represents a tab in the reorder request
    /// </summary>
    public class TabOrderItem
    {
        /// <summary>
        /// The ID of the tab
        /// </summary>
        public string TabId { get; set; } = string.Empty;
    }
}
