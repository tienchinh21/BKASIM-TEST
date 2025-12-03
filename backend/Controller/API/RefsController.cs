using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.DTOs.Refs;
using MiniAppGIBA.Models.Request.Refs;
using MiniAppGIBA.Services.Refs;

namespace MiniAppGIBA.Controller.API
{
    [Route("api/refs")]
    [ApiController]
    public class RefsController : ControllerBase
    {
        private readonly IRefService _refService;
        private readonly ILogger<RefsController> _logger;

        public RefsController(IRefService refService, ILogger<RefsController> logger)
        {
            _refService = refService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo ref mới
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateRef([FromBody] CreateRefRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _refService.CreateRefAsync(userZaloId, request);

                return Ok(new
                {
                    code = 0,
                    message = "Tạo ref thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ref");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách ref (cả đã gửi và đã nhận)
        /// </summary>
        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetRefs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] byte? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? type = null) // "sent" hoặc "received", null = tất cả
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _refService.GetMyRefsAsync(userZaloId, page, pageSize, keyword, status, fromDate, toDate, type);

                return Ok(new
                {
                    code = 0,
                    message = "Lấy danh sách ref thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật giá trị ref (người nhận nhập giá trị)
        /// </summary>
        [HttpPut("update-value/{refId}")]
        [Authorize]
        public async Task<IActionResult> UpdateRefValue(string refId, [FromBody] UpdateRefValueRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra ref có thuộc về user này không
                var refEntity = await _refService.GetRefByIdAsync(refId);
                if (refEntity == null)
                {
                    return NotFound(new { message = "Không tìm thấy ref" });
                }

                // Type 0: RefTo phải là userZaloId
                // Type 1: RefTo = null, không thể update value (chỉ Type 0 mới update được)
                if (refEntity.Type == 0 && refEntity.RefTo != userZaloId)
                {
                    return Forbid("Bạn không có quyền cập nhật ref này");
                }
                
                if (refEntity.Type == 1)
                {
                    return BadRequest(new { message = "Ref Type 1 (gửi cho bên ngoài) không thể cập nhật giá trị" });
                }

                var result = await _refService.UpdateRefValueAsync(refId, request);

                return Ok(new
                {
                    code = 0,
                    message = "Cập nhật giá trị ref thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ref value for {RefId}", refId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết ref
        /// </summary>
        [HttpGet("detail/{refId}")]
        [Authorize]
        public async Task<IActionResult> GetRefDetail(string refId)
        {
            try
            {
                var result = await _refService.GetRefByIdAsync(refId);
                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy ref" });
                }

                return Ok(new
                {
                    code = 0,
                    message = "Lấy chi tiết ref thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ref detail {RefId}", refId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
