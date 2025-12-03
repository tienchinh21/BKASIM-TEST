using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Request.SystemSettings;
using MiniAppGIBA.Models.Response.SystemSettings;
using MiniAppGIBA.Services.SystemSettings;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]

    public class BehaviorRulesController(
        IBehaviorRulesService behaviorRulesService,
        IWebHostEnvironment env,
        ILogger<BehaviorRulesController> logger) : BaseAPIController
    {

        [HttpGet]
        public async Task<IActionResult> GetBehaviorRulesFile(EBehaviorRuleType type, string? groupId = null)
        {
            try
            {
                var result = await behaviorRulesService.GetBehaviorRulesDetailAsync(type, groupId);

                if (result.Success && result.Data != null && !string.IsNullOrEmpty(result.Data.FileUrl))
                {
                    result.Data.FileUrl = ToFullUrl(result.Data.FileUrl);
                }

                return Success(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching behavior rules file for type: {Type}, groupId: {GroupId}", type, groupId);
                return Error("Có lỗi xảy ra khi lấy thông tin quy tắc ứng xử", 500);
            }
        }

        [HttpGet("file")]
        public async Task<IActionResult> GetBehaviorRulesFileContent(EBehaviorRuleType type, string? groupId = null)
        {
            try
            {
                var result = await behaviorRulesService.GetBehaviorRulesDetailAsync(type, groupId);

                if (!result.Success || result.Data == null || string.IsNullOrEmpty(result.Data.FileUrl))
                {
                    return BadRequest("File not found");
                }

                // var fileUrl = ToFullUrl(result.Data.FileUrl);
                var fileUrl = result.Data.FileUrl;


                // Handle relative path - convert to absolute file path
                // if (!fileUrl.StartsWith("http://") && !fileUrl.StartsWith("https://"))
                // {
                // Remove leading slash if present for Path.Combine
                if (fileUrl.StartsWith("/"))
                    fileUrl = fileUrl.Substring(1);

                var filePath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, fileUrl);

                if (!System.IO.File.Exists(filePath))
                {
                    logger.LogWarning("File not found at path: {FilePath}", filePath);
                    return NotFound("File not found on server");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(filePath);
                var fileName = Path.GetFileName(filePath);

                // Add CORS headers
                Response.Headers["Access-Control-Allow-Origin"] = "*";
                Response.Headers["Access-Control-Allow-Methods"] = "GET";
                Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";

                return File(fileBytes, contentType, fileName);
                // }
                // else
                // {
                //     // For external URLs, redirect to the URL
                //     return Redirect(fileUrl);
                // }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error serving behavior rules file for type: {Type}, groupId: {GroupId}", type, groupId);
                return StatusCode(500, "Internal server error");
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

        private string ToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // If already full URL, return as is
            if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
                return relativePath;

            // Ensure path starts with /
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            // Build full URL
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return baseUrl + relativePath;
        }



    }
    public class SaveRuleUrlRequest
    {
        public string Url { get; set; } = string.Empty;
    }
}
