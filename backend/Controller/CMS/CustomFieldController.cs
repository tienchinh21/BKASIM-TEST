using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;
using MiniAppGIBA.Services.CustomFields;

namespace MiniAppGIBA.Controller.CMS
{
    /// <summary>
    /// CMS Controller for managing custom fields within tabs
    /// Handles CRUD operations for fields used in membership registration forms
    /// </summary>
    [Authorize]
    public class CustomFieldController : BaseCMSController
    {
        private readonly ICustomFieldService _customFieldService;
        private readonly ILogger<CustomFieldController> _logger;

        public CustomFieldController(
            ICustomFieldService customFieldService,
            ILogger<CustomFieldController> logger)
        {
            _customFieldService = customFieldService;
            _logger = logger;
        }

        /// <summary>
        /// Display the custom fields management page for a tab
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string tabId)
        {
            try
            {
                // Only GIBA (super admin) can manage custom fields
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ GIBA mới có quyền quản lý các trường tùy chỉnh!");
                    return RedirectToAction("Index", "CustomFieldTab");
                }

                if (string.IsNullOrEmpty(tabId))
                {
                    SetErrorMessage("ID tab không được để trống");
                    return RedirectToAction("Index", "CustomFieldTab");
                }

                ViewBag.TabId = tabId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading custom fields index for tab {TabId}", tabId);
                SetErrorMessage("Có lỗi xảy ra khi tải trang quản lý trường tùy chỉnh");
                return RedirectToAction("Index", "CustomFieldTab");
            }
        }

        /// <summary>
        /// API endpoint to get all fields for a tab
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFieldsByTab(string tabId)
        {
            try
            {
                if (string.IsNullOrEmpty(tabId))
                {
                    return Json(new { success = false, message = "ID tab không được để trống" });
                }

                var fields = await _customFieldService.GetFieldsByTabAsync(tabId);

                return Json(new
                {
                    success = true,
                    data = fields.OrderBy(f => f.DisplayOrder).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields for tab {TabId}", tabId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách trường" });
            }
        }

        /// <summary>
        /// API endpoint to get all fields for an entity (optionally filtered by tab)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFieldsByEntity(ECustomFieldEntityType entityType, string entityId, string? tabId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(entityId))
                {
                    return Json(new { success = false, message = "ID thực thể không được để trống" });
                }

                var fields = await _customFieldService.GetFieldsByEntityAsync(entityType, entityId, tabId);

                return Json(new
                {
                    success = true,
                    data = fields.OrderBy(f => f.DisplayOrder).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields for entity type {EntityType}, entity ID {EntityId}", entityType, entityId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách trường" });
            }
        }

        /// <summary>
        /// API endpoint to create a new field within a tab
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateField([FromBody] CreateCustomFieldRequest request)
        {
            try
            {
                // Only GIBA can create fields
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền tạo trường tùy chỉnh!" });
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (string.IsNullOrWhiteSpace(request.FieldName))
                {
                    return Json(new { success = false, message = "Tên trường không được để trống" });
                }

                if (string.IsNullOrEmpty(request.EntityId))
                {
                    return Json(new { success = false, message = "ID thực thể không được để trống" });
                }

                if (request.DisplayOrder < 0)
                {
                    return Json(new { success = false, message = "Thứ tự hiển thị phải là số không âm" });
                }

                var createdField = await _customFieldService.CreateFieldAsync(request);

                _logger.LogInformation("Field created successfully for entity {EntityId}: {FieldName}",
                    request.EntityId, request.FieldName);

                return Json(new
                {
                    success = true,
                    message = "Tạo trường thành công",
                    data = createdField
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument validation error when creating field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating field");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo trường" });
            }
        }

        /// <summary>
        /// API endpoint to update an existing field
        /// </summary>
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateField([FromBody] UpdateCustomFieldRequest request)
        {
            try
            {
                // Only GIBA can update fields
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền cập nhật trường tùy chỉnh!" });
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (string.IsNullOrEmpty(request.Id))
                {
                    return Json(new { success = false, message = "ID trường không được để trống" });
                }

                if (string.IsNullOrWhiteSpace(request.FieldName))
                {
                    return Json(new { success = false, message = "Tên trường không được để trống" });
                }

                if (request.DisplayOrder < 0)
                {
                    return Json(new { success = false, message = "Thứ tự hiển thị phải là số không âm" });
                }

                var updatedField = await _customFieldService.UpdateFieldAsync(request);

                _logger.LogInformation("Field updated successfully: {FieldId}", request.Id);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật trường thành công",
                    data = updatedField
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Field not found when updating");
                return Json(new { success = false, message = "Không tìm thấy trường" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument validation error when updating field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trường" });
            }
        }

        /// <summary>
        /// API endpoint to delete a field
        /// </summary>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteField(string fieldId)
        {
            try
            {
                // Only GIBA can delete fields
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa trường tùy chỉnh!" });
                }

                if (string.IsNullOrEmpty(fieldId))
                {
                    return Json(new { success = false, message = "ID trường không được để trống" });
                }

                var result = await _customFieldService.DeleteFieldAsync(fieldId);

                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy trường" });
                }

                _logger.LogInformation("Field deleted successfully: {FieldId}", fieldId);

                return Json(new
                {
                    success = true,
                    message = "Xóa trường thành công"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument validation error when deleting field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting field");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting field {FieldId}", fieldId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa trường" });
            }
        }

        /// <summary>
        /// API endpoint to reorder fields within a tab
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderFields([FromBody] ReorderFieldsRequest request)
        {
            try
            {
                // Only GIBA can reorder fields
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền sắp xếp lại trường!" });
                }

                if (request == null || string.IsNullOrEmpty(request.TabId))
                {
                    return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ" });
                }

                if (request.Fields == null || request.Fields.Count == 0)
                {
                    return Json(new { success = false, message = "Danh sách trường không được để trống" });
                }

                var reordering = request.Fields
                    .Select((field, index) => (FieldId: field.FieldId, Order: index + 1))
                    .ToList();

                var result = await _customFieldService.ReorderFieldsAsync(request.TabId, reordering);

                if (!result)
                {
                    return Json(new { success = false, message = "Không thể sắp xếp lại trường" });
                }

                _logger.LogInformation("Fields reordered successfully for tab {TabId}", request.TabId);

                return Json(new
                {
                    success = true,
                    message = "Sắp xếp lại trường thành công"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument validation error when reordering fields");
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when reordering fields");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering fields for tab {TabId}", request?.TabId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi sắp xếp lại trường" });
            }
        }
    }

    /// <summary>
    /// Request model for reordering fields
    /// </summary>
    public class ReorderFieldsRequest
    {
        /// <summary>
        /// The ID of the tab containing the fields
        /// </summary>
        public string TabId { get; set; } = string.Empty;

        /// <summary>
        /// List of fields with their new order
        /// </summary>
        public List<FieldOrderItem> Fields { get; set; } = [];
    }

    /// <summary>
    /// Represents a field in the reorder request
    /// </summary>
    public class FieldOrderItem
    {
        /// <summary>
        /// The ID of the field
        /// </summary>
        public string FieldId { get; set; } = string.Empty;
    }
}
/// /// /// /// 