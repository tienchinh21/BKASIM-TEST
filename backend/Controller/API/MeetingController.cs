using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Models.Queries.Meetings;
using MiniAppGIBA.Services.Meetings;
using MiniAppGIBA.Models.Request.Meetings;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : BaseAPIController
    {
        private readonly IMeetingService _meetingService;

        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] MeetingQueryParams query)
        {
            try
            {
                var roleId = User.GetRoleId().FirstOrDefault() ?? string.Empty;
                var userId = GetCurrentUserId() ?? string.Empty;
                var result = await _meetingService.GetPage(query, roleId, userId);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Items = result.Items,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    Page = result.Page,
                    PageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch họp",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                var result = await _meetingService.GetByIdAsync(id);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Item = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy chi tiết lịch họp",
                    Error = ex.Message
                });
            }
        }

        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] MeetingRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var roleName = User.GetRoles().FirstOrDefault() ?? string.Empty;

                request.CreatedBy = userId ?? string.Empty;
                request.RoleName = roleName;

                var result = await _meetingService.CreateAsync(request);

                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo lịch họp thành công",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi tạo lịch họp",
                    Error = ex.Message
                });
            }
        }

        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MeetingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new
                    {
                        Code = 1,
                        Message = "Id không được để trống"
                    });
                }

                var userId = GetCurrentUserId();
                var roleName = User.GetRoles().FirstOrDefault() ?? string.Empty;

                request.CreatedBy = userId ?? string.Empty;
                request.RoleName = roleName;

                var result = await _meetingService.UpdateAsync(id, request);

                if (result)
                {
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Cập nhật lịch họp thành công",
                        Success = result
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Code = 1,
                        Message = "Cập nhật lịch họp thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi cập nhật lịch họp",
                    Error = ex.Message
                });
            }
        }

        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new
                    {
                        Code = 1,
                        Message = "Id không được để trống"
                    });
                }

                var result = await _meetingService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Xóa lịch họp thành công",
                        Success = result
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Code = 1,
                        Message = "Xóa lịch họp thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi xóa lịch họp",
                    Error = ex.Message
                });
            }
        }
    }
}

