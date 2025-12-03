using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Queries.Fields;
using MiniAppGIBA.Models.Request.Fields;
using MiniAppGIBA.Services.Fields;

namespace MiniAppGIBA.Controller.API
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/fields")]
    [ApiController]
    public class FieldsController : BaseAPIController
    {
        private readonly IFieldService _fieldService;
        private readonly ILogger<FieldsController> _logger;

        public FieldsController(
            IFieldService fieldService,
            ILogger<FieldsController> logger)
        {
            _fieldService = fieldService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách lĩnh vực (có phân trang và tìm kiếm)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFields([FromQuery] FieldQueryParameters queryParameters)
        {
            try
            {
                if (!IsAuthenticated() || !IsAdmin())
                {
                    return Error("Không có quyền truy cập", 403);
                }

                var result = await _fieldService.GetFieldsAsync(queryParameters);
                return Success(result);
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Get fields failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Lấy thông tin lĩnh vực theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetField(string id)
        {
            try
            {
                if (!IsAuthenticated() || !IsAdmin())
                {
                    return Error("Không có quyền truy cập", 403);
                }

                var field = await _fieldService.GetFieldByIdAsync(id);
                if (field == null)
                {
                    return Error("Không tìm thấy lĩnh vực", 404);
                }

                return Success(field);
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Get field failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Tạo lĩnh vực mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateField([FromBody] CreateFieldRequest request)
        {
            try
            {
                if (!IsAuthenticated() || !IsAdmin())
                {
                    return Error("Không có quyền truy cập", 403);
                }

                if (!ModelState.IsValid)
                {
                    return Error("Dữ liệu không hợp lệ", 400);
                }

                var field = await _fieldService.CreateFieldAsync(request);
                return Success(field, "Tạo lĩnh vực thành công!");
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Create field failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Cập nhật lĩnh vực
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateField(string id, [FromBody] UpdateFieldRequest request)
        {
            try
            {
                if (!IsAuthenticated() || !IsAdmin())
                {
                    return Error("Không có quyền truy cập", 403);
                }

                if (!ModelState.IsValid)
                {
                    return Error("Dữ liệu không hợp lệ", 400);
                }

                request.Id = id;
                var field = await _fieldService.UpdateFieldAsync(request);
                return Success(field, "Cập nhật lĩnh vực thành công!");
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Update field failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Xóa lĩnh vực
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteField(string id)
        {
            try
            {
                if (!IsAuthenticated() || !IsAdmin())
                {
                    return Error("Không có quyền truy cập", 403);
                }

                await _fieldService.DeleteFieldAsync(id);
                return Success(new { }, "Xóa lĩnh vực thành công!");
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Delete field failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Lấy danh sách lĩnh vực đang hoạt động (cho Mini App)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveFields()
        {
            try
            {
                var fields = await _fieldService.GetActiveFieldsAsync();
                return Success(fields);
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Get active fields failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active fields");
                return Error("Internal Server Error", 500);
            }
        }
    }
}
