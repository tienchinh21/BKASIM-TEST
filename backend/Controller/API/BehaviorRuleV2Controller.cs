using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Controller.API;
using MiniAppGIBA.Models.Request.Rules;
using MiniAppGIBA.Services.Rules;
using System.IO;
using MiniAppGIBA.Base.Interface;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class BehaviorRuleV2Controller(
        IBehaviorRuleService behaviorRuleService,
        IWebHostEnvironment env,
        ILogger<BehaviorRuleV2Controller> logger,
        IUrl url
        
    ) : BaseAPIController
    {
        private readonly IUrl _url = url;
        /// <summary>
        /// Tạo mới Behavior Rule (v2): hỗ trợ TEXT/FILE, APP/GROUP
        /// </summary>
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = CTRole.Club + ","+CTRole.GIBA)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateBehaviorRuleRequest request)
        {
            try
            {
                var entity = await behaviorRuleService.CreateAsync(request, env, HttpContext);

                return Success(new
                {
                    id = entity.Id,
                    title = entity.Title,
                    contentType = entity.ContentType,
                    type = entity.Type,
                    content = entity.Content,
                    groupId = entity.GroupId,
                    sortOrder = entity.SortOrder,
                    createdDate = entity.CreatedDate,
                    updatedDate = entity.UpdatedDate,
                    isActive = entity.IsActive
                });
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating behavior rule v2");
                return Error("Có lỗi xảy ra khi tạo Behavior Rule", 500);
            }
        }

        /// <summary>
        /// Trả metadata theo page/type (không trả binary) để FE dùng cho cập nhật.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("pageInfo")]
        public async Task<IActionResult> GetPageInfo([FromQuery] int page, [FromQuery] string type, [FromQuery] string? groupId = null)
        {
            try
            {
                if (page <= 0)
                {
                    return Error("Page phải >= 1", 400);
                }

                var (rule, totalPages) = await behaviorRuleService.GetByPageAsync(type, groupId, page);
                if (totalPages == 0 || rule == null)
                {
                    return Success(new
                    {
                        id = (string?)null,
                        title = (string?)null,
                        contentType = (string?)null,
                        type = (string?)null,
                        content = (string?)null,
                        groupId = (string?)null,
                        sortOrder = (int?)null,
                        totalPages = 0,
                        currentPage = 0
                    }, "Không có dữ liệu");
                }
                if(rule.ContentType == "FILE") rule.Content = await _url.ToFullUrl(rule.Content,HttpContext);
                var currentPage = Math.Max(1, Math.Min(page, totalPages));
                return Success(new
                {
                    id = rule.Id,
                    title = rule.Title,
                    contentType = rule.ContentType,
                    type = rule.Type,
                    content = rule.Content,
                    groupId = rule.GroupId,
                    sortOrder = rule.SortOrder,
                    totalPages,
                    currentPage
                });
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting behavior rule v2 page info");
                return Error("Có lỗi xảy ra khi lấy dữ liệu", 500);
            }
        }

        /// <summary>
        /// Lấy Behavior Rule theo Id để chỉnh sửa
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var rule = await behaviorRuleService.GetByIdAsync(id);
                if (rule == null)
                {
                    return Error("Không tìm thấy Behavior Rule", 404);
                }

                return Success(new
                {
                    id = rule.Id,
                    title = rule.Title,
                    contentType = rule.ContentType,
                    type = rule.Type,
                    content = rule.Content,
                    groupId = rule.GroupId,
                    sortOrder = rule.SortOrder,
                    createdDate = rule.CreatedDate,
                    updatedDate = rule.UpdatedDate,
                    isActive = rule.IsActive
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting behavior rule by id {Id}", id);
                return Error("Có lỗi xảy ra khi lấy dữ liệu", 500);
            }
        }

        /// <summary>
        /// Cập nhật Behavior Rule theo Id.
        /// - TEXT: lưu nội dung text.
        /// - FILE: xóa file cũ, upload file mới và lưu URL.
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateBehaviorRuleRequest request)
        {
            try
            {
                var entity = await behaviorRuleService.UpdateAsync(request, env, HttpContext);

                return Success(new
                {
                    id = entity.Id,
                    title = entity.Title,
                    contentType = entity.ContentType,
                    type = entity.Type,
                    content = entity.Content,
                    groupId = entity.GroupId,
                    sortOrder = entity.SortOrder,
                    createdDate = entity.CreatedDate,
                    updatedDate = entity.UpdatedDate,
                    isActive = entity.IsActive
                });
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating behavior rule v2");
                return Error("Có lỗi xảy ra khi cập nhật Behavior Rule", 500);
            }
        }

        /// <summary>
        /// Xóa Behavior Rule theo Id. Nếu là FILE sẽ xóa cả file vật lý.
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                var ok = await behaviorRuleService.DeleteAsync(id, env);
                if (ok)
                {
                    return Success(new { id }, "Đã xóa Behavior Rule");
                }
                return Error("Xóa không thành công", 400);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting behavior rule v2 {Id}", id);
                return Error("Có lỗi xảy ra khi xóa Behavior Rule", 500);
            }
        }

        /// <summary>
        /// Lấy Behavior Rule theo page và type.
        /// - Nếu ContentType = FILE: trả về file binary giống controller BehaviorRulesController
        /// - Nếu ContentType = TEXT: trả về nội dung text cùng metadata (totalPages, currentPage)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page, [FromQuery] string type, [FromQuery] string? groupId = null)
        {
            try
            {
                if (page <= 0)
                {
                    return Error("Page phải >= 1", 400);
                }

                var (rule, totalPages) = await behaviorRuleService.GetByPageAsync(type, groupId, page);

                if (totalPages == 0 || rule == null)
                {
                    return Success(new
                    {
                        content = (string?)null,
                        contentType = (string?)null,
                        title = (string?)null,
                        totalPages = 0,
                        currentPage = 0
                    }, "Không có dữ liệu");
                }

                // Determine actual current page after clamping
                var currentPage = Math.Max(1, Math.Min(page, totalPages));

                if (string.Equals(rule.ContentType, "FILE", StringComparison.OrdinalIgnoreCase))
                {
                    var fileUrl = rule.Content ?? string.Empty;
                    if (string.IsNullOrEmpty(fileUrl))
                    {
                        return Error("Không tìm thấy file", 404);
                    }

                    if (fileUrl.StartsWith("/"))
                        fileUrl = fileUrl.Substring(1);

                    var filePath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, fileUrl);

                    if (!System.IO.File.Exists(filePath))
                    {
                        return Error("File không tồn tại trên server", 404);
                    }

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    var contentTypeHeader = GetContentType(filePath);
                    var fileName = Path.GetFileName(filePath);

                    // CORS headers
                    Response.Headers["Access-Control-Allow-Origin"] = "*";
                    Response.Headers["Access-Control-Allow-Methods"] = "GET";
                    Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";

                    // Note: binary response; client can call another endpoint to get metadata if needed
                    return File(fileBytes, contentTypeHeader, fileName);
                }
                else
                {
                    // TEXT response with metadata
                    return Success(new
                    {
                        content = rule.Content,
                        contentType = rule.ContentType,
                        title = rule.Title,
                        totalPages,
                        currentPage
                    });
                }
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting behavior rule v2 by page");
                return Error("Có lỗi xảy ra khi lấy dữ liệu", 500);
            }
        }

        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}