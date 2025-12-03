using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Showcase;
using MiniAppGIBA.Services.ShowCase;
using MiniAppGIBA.Models.Request.Showcase;
namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ShowcaseController : BaseAPIController
    {

        private readonly IShowcaseService _showcaseService;

        public ShowcaseController(IShowcaseService showcaseService)
        {
            _showcaseService = showcaseService;
        }

        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] ShowcaseQueryParams query)
        {
            try
            {
                var roleId = User.GetRoleId().FirstOrDefault() ?? string.Empty;
                var userId = GetCurrentUserId() ?? string.Empty;
                var result = await _showcaseService.GetPage(query, roleId, userId);
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
                    Message = "Có lỗi xảy ra khi lấy danh sách showcase",
                    Error = ex.Message
                });
            }
        }
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                var result = await _showcaseService.GetByIdAsync(id);
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
                    Message = "Có lỗi xảy ra khi lấy chi tiết showcase",
                    Error = ex.Message
                });
            }
        }

        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ShowcaseRequest request)
        {
            try
            {
                // Get current user info from claims
                var userId = GetCurrentUserId();
                var roleName = User.GetRoles().FirstOrDefault() ?? string.Empty;

                // Set CreatedBy and RoleName
                request.CreatedBy = userId ?? string.Empty;
                request.RoleName = roleName;

                var result = await _showcaseService.CreateAsync(request);

                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo showcase thành công",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi tạo showcase",
                    Error = ex.Message
                });
            }
        }
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ShowcaseRequest request)
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

                // Get current user info from claims
                var userId = GetCurrentUserId();
                var roleName = User.GetRoles().FirstOrDefault() ?? string.Empty;

                // Set CreatedBy and RoleName (for consistency, though CreatedBy might not be updated)
                request.CreatedBy = userId ?? string.Empty;
                request.RoleName = roleName;

                var result = await _showcaseService.UpdateAsync(id, request);

                if (result)
                {
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Cập nhật showcase thành công",
                        Success = result
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Code = 1,
                        Message = "Cập nhật showcase thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi cập nhật showcase",
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

                var result = await _showcaseService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Xóa showcase thành công",
                        Success = result
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Code = 1,
                        Message = "Xóa showcase thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi xóa showcase",
                    Error = ex.Message
                });
            }
        }
    }
}