using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Request.SystemSettings;
using MiniAppGIBA.Services.Commons;
using MiniAppGIBA.Services.SystemSettings;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Request.Rules;
using MiniAppGIBA.Services.Rules;
using MiniAppGIBA.Base.Interface;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Controller.CMS
{
    
    [Route("Setting")]
    public class SettingController : BaseCMSController
    {
        private readonly ISystemConfigService _systemConfigService;
        private readonly IBehaviorRulesService _behaviorRulesService;
        private readonly IAppBriefService _appBriefService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SettingController> _logger;
        private readonly IBehaviorRuleService _behaviorRuleService;
        private readonly IUnitOfWork _unitOfWork;

        public SettingController(
            ISystemConfigService systemConfigService,
            IBehaviorRulesService behaviorRulesService,
            IAppBriefService appBriefService,
            ILogger<SettingController> logger,
            IWebHostEnvironment env,
            IBehaviorRuleService behaviorRuleService,
            IUnitOfWork unitOfWork)
        {
            _systemConfigService = systemConfigService;
            _behaviorRulesService = behaviorRulesService;
            _appBriefService = appBriefService;
            _logger = logger;
            _env = env;
            _behaviorRuleService = behaviorRuleService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Trang cài đặt hệ thống chính
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [Authorize]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        /// <summary>
        /// Trang cấu hình quy tắc ứng xử
        /// </summary>
        [HttpGet("BehaviorRules")]
        [Authorize]
        public async Task<IActionResult> BehaviorRulesPage(string type, string groupId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                var rule = await _systemConfigService.GetByTypeAsync("BehaviorRules");
                
                return View("BehaviorRules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading behavior rules");
                ViewBag.RuleContent = "";
                return View("BehaviorRules");
            }
        }

        /// <summary>
        /// API: Lưu quy tắc ứng xử (legacy text)
        /// </summary>
        [HttpPost("SaveBehaviorRules")]
        [Authorize]
        public async Task<IActionResult> SaveBehaviorRules([FromBody] SaveRuleRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Content))
                {
                    return Json(new { success = false, message = "Nội dung quy tắc không được để trống" });
                }

                await _behaviorRulesService.SaveBehaviorRulesUrlAsync(request.Content);
                return Json(new { success = true, message = "Lưu quy tắc ứng xử thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving behavior rules");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lưu quy tắc" });
            }
        }

        /// <summary>
        /// API: Upload tài liệu quy tắc ứng xử
        /// </summary>
        [HttpPost("UploadBehaviorRulesFile")]
        [Authorize]
        [RequestSizeLimit(104857600)] // 100 MB
        public async Task<IActionResult> UploadBehaviorRulesFile(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn tệp để tải lên" });
                }

                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "behavior_rules");
                Directory.CreateDirectory(uploadDir);

                var results = new List<object>();
                foreach (var file in files.Take(20))
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (!allowedExt.Contains(ext))
                    {
                        return Json(new { success = false, message = $"Định dạng không hỗ trợ: {ext}" });
                    }

                    var safeName = Path.GetFileNameWithoutExtension(file.FileName);
                    var fileName = $"{safeName}_{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var link = $"/uploads/behavior_rules/{fileName}";
                    results.Add(new { name = file.FileName, link });
                }

                // Lưu metadata vào SystemConfig (tùy chọn)
                var json = System.Text.Json.JsonSerializer.Serialize(results);
                await _systemConfigService.UpsertByTypeAsync("BehaviorRulesFile", json);

                return Json(new { success = true, message = "Tải lên thành công", data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading behavior rules files");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lên" });
            }
        }

        /// <summary>
        /// API: Lấy thông tin tài liệu quy tắc ứng xử (có thể chưa có)
        /// </summary>
        [HttpGet("GetBehaviorRulesFile")]
        [Authorize]
        public async Task<IActionResult> GetBehaviorRulesFile(EBehaviorRuleType type, string groupId)
        {
            try
            {
                var result = await _behaviorRulesService.GetBehaviorRulesDetailAsync(type, groupId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching behavior rules file");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// API: Upload file tạm thời (chỉ để preview)
        /// </summary>
        [HttpPost("UploadBehaviorRulesFileTemp")]
        [RequestSizeLimit(104857600)] // 100 MB
        [Authorize]
        public async Task<IActionResult> UploadBehaviorRulesFileTemp(IFormFile file, string? groupId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file để tải lên" });
                }

                var result = await _behaviorRulesService.UploadFileTempAsync(file, _env.WebRootPath, groupId);
                
                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        data = new { name = result.FileName, url = result.FileUrl, size = result.FileSize }
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lên file" });
            }
        }

        /// <summary>
        /// API: Lưu file từ temp vào DB
        /// </summary>
        [HttpPost("SaveBehaviorRulesFile")]
        [Authorize]
        public async Task<IActionResult> SaveBehaviorRulesFile([FromBody] SaveBehaviorRulesFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.TempFilePath))
                {
                    return Json(new { success = false, message = "Đường dẫn file không hợp lệ" });
                }

                var result = await _behaviorRulesService.SaveFileFromTempAsync(request.TempFilePath, _env.WebRootPath, request.GroupId);
                
                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        data = new { name = result.FileName, url = result.FileUrl }
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi lưu file" });
            }
        }

        /// <summary>
        /// API: Thay đổi file quy tắc ứng xử (xóa cũ, upload mới) - Legacy
        /// </summary>
        [HttpPost("ReplaceBehaviorRulesFile")]
        [RequestSizeLimit(104857600)] // 100 MB
        [Authorize]
        public async Task<IActionResult> ReplaceBehaviorRulesFile(IFormFile file, string? groupId = null)
        {
            try
            {
                // Prepare request for service
                var adminGroups = await GetAdminGroupsAsync();
                var request = new BehaviorRulesUploadRequest
                {
                    File = file,
                    IsSuperAdmin = IsSuperAdmin(),
                    UserId = GetCurrentUserId(),
                    AdminGroups = adminGroups,
                    WebRootPath = _env.WebRootPath,
                    GroupId = groupId // Pass groupId to service
                };

                // Call service
                var result = await _behaviorRulesService.ReplaceFileAsync(request);

                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        data = new { name = result.FileName, url = result.FileUrl }
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing behavior rules file");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi tài liệu" });
            }
        }

        /// <summary>
        /// API: Xóa quy tắc ứng xử của group
        /// </summary>
        [HttpDelete("DeleteBehaviorRulesForGroup")]
        [Authorize]
        public async Task<IActionResult> DeleteBehaviorRulesForGroup(string groupId)
        {
            try
            {
                var result = await _behaviorRulesService.DeleteBehaviorRulesForGroupAsync(groupId, _env.WebRootPath);
                
                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting behavior rules for group {GroupId}", groupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài liệu" });
            }
        }

        /// <summary>
        /// API: Xóa quy tắc ứng xử của super admin
        /// </summary>
        [HttpDelete("DeleteBehaviorRules")]
        [Authorize]
        public async Task<IActionResult> DeleteBehaviorRules()
        {
            try
            {
                await _behaviorRulesService.SaveBehaviorRulesUrlAsync("");
                return Json(new { success = true, message = "Đã xóa tài liệu quy tắc ứng xử" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting behavior rules");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài liệu" });
            }
        }

        /// <summary>
        /// Get admin's groups from GroupPermission service
        /// </summary>
        private Task<List<string>?> GetAdminGroupsAsync()
        {
            try
            {
                if (IsSuperAdmin())
                {
                    // SUPER_ADMIN has access to all groups - return null for superadmin folder
                    return Task.FromResult<List<string>?>(null);
                }

                // For ADMIN, get their assigned group IDs
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Task.FromResult<List<string>?>(new List<string> { "default" });

                // Get groups from claims or GroupPermission service
                var userClaims = User.Claims.Where(c => c.Type == "GroupPermission").Select(c => c.Value).ToList();
                
                if (userClaims.Any())
                {
                    return Task.FromResult<List<string>?>(userClaims);
                }

                // Fallback to default if no groups assigned
                return Task.FromResult<List<string>?>(new List<string> { "default" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin groups for user");
                return Task.FromResult<List<string>?>(new List<string> { "default" });
            }
        }

        #region App Brief

        /// <summary>
        /// Trang quản lý Brief Giới Thiệu App
        /// </summary>
        [HttpGet("AppBrief")]
        [Authorize]
        public IActionResult AppBrief()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        /// <summary>
        /// API: Lấy nội dung App Brief
        /// </summary>
        [HttpGet("GetAppBrief")]
        public async Task<IActionResult> GetAppBrief()
        {
            try
            {
                var result = await _appBriefService.GetAppBriefAsync();
                if (result == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nội dung" });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting App Brief");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy nội dung" });
            }
        }

        /// <summary>
        /// API: Lưu nội dung HTML
        /// </summary>
        [HttpPost("SaveAppBriefHtml")]
        [Authorize]
        public async Task<IActionResult> SaveAppBriefHtml([FromBody] SaveAppBriefHtmlRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Content))
                {
                    return Json(new { success = false, message = "Nội dung không được để trống" });
                }

                // Get existing config to delete old PDF if exists
                var existing = await _appBriefService.GetAppBriefAsync();
                if (existing != null && existing.IsPdf && !string.IsNullOrEmpty(existing.PdfUrl))
                {
                    // Delete old PDF file
                    var oldFilePath = Path.Combine(_env.WebRootPath, existing.PdfUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                            _logger.LogInformation("Deleted old PDF file: {FilePath}", oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old PDF file: {FilePath}", oldFilePath);
                        }
                    }
                }

                await _appBriefService.SaveHtmlContentAsync(request.Content);
                return Json(new { success = true, message = "Lưu nội dung thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving App Brief HTML");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lưu nội dung" });
            }
        }

        /// <summary>
        /// API: Upload file PDF tạm thời (chỉ để preview)
        /// </summary>
        [HttpPost("UploadAppBriefPdfTemp")]
        [RequestSizeLimit(52428800)] // 50 MB
        [Authorize]
        public async Task<IActionResult> UploadAppBriefPdfTemp(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file PDF để tải lên" });
                }

                var result = await _appBriefService.UploadPdfTempAsync(file, _env.WebRootPath);
                return Json(new 
                { 
                    success = true, 
                    message = "Tải lên file PDF thành công. Vui lòng bấm 'Lưu' để lưu vào hệ thống.",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lên file" });
            }
        }

        /// <summary>
        /// API: Lưu file PDF từ temp vào DB
        /// </summary>
        [HttpPost("SaveAppBriefPdf")]
        [Authorize]
        public async Task<IActionResult> SaveAppBriefPdf([FromBody] SaveAppBriefPdfRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.TempFilePath))
                {
                    return Json(new { success = false, message = "Đường dẫn file không hợp lệ" });
                }

                var result = await _appBriefService.SavePdfFromTempAsync(request.TempFilePath, _env.WebRootPath);
                return Json(new 
                { 
                    success = true, 
                    message = "Lưu file PDF thành công",
                    data = result
                });
            }
            catch (FileNotFoundException)
            {
                return Json(new { success = false, message = "File không tồn tại. Vui lòng upload lại." });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi lưu file" });
            }
        }

        /// <summary>
        /// API: Xóa file PDF
        /// </summary>
        [HttpDelete("DeleteAppBriefPdf")]
        [Authorize]
        public async Task<IActionResult> DeleteAppBriefPdf()
        {
            try
            {
                await _appBriefService.DeletePdfAsync(_env.WebRootPath);
                return Json(new { success = true, message = "Đã xóa file PDF thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting App Brief PDF");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa file" });
            }
        }

        #endregion

        #region Behavior Rules V2 Form Actions

        /// <summary>
        /// Generic action to load behavior rule form (for both create and edit)
        /// Used by GROUP behavior rules modal
        /// </summary>
        [HttpGet("GetBehaviorRuleForm")]
        [Authorize]
        public async Task<IActionResult> GetBehaviorRuleForm(string? id = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    // Create mode - return empty form
                    // The type and groupId will be set by JavaScript after form loads
                    ViewBag.IsEdit = false;
                    return PartialView("~/Views/Setting/Partials/_BehaviorRuleForm.cshtml", new CreateBehaviorRuleRequest
                    {
                        ContentType = "TEXT",
                        SortOrder = 0
                    });
                }
                else
                {
                    // Edit mode - load the existing rule
                    var rule = await _behaviorRuleService.GetByIdAsync(id);
                    if (rule == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy quy tắc ứng xử" });
                    }

                    ViewBag.IsEdit = true;
                    ViewBag.RuleId = id;
                    ViewBag.Type = rule.Type;
                    ViewBag.GroupId = rule.GroupId;

                    // Get group name if applicable
                    if (!string.IsNullOrEmpty(rule.GroupId))
                    {
                        var groupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.Group>();
                        var group = await groupRepo.AsQueryable()
                            .Where(g => g.Id == rule.GroupId)
                            .FirstOrDefaultAsync();
                        ViewBag.GroupName = group?.GroupName ?? "Unknown";
                    }
                    else
                    {
                        ViewBag.GroupName = "Không";
                    }

                    // Map to model
                    var model = new CreateBehaviorRuleRequest
                    {
                        ContentType = rule.ContentType,
                        Type = rule.Type,
                        GroupId = rule.GroupId,
                        Content = rule.Content,
                        Title = rule.Title,
                        SortOrder = rule.SortOrder
                    };

                    return PartialView("~/Views/Setting/Partials/_BehaviorRuleForm.cshtml", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading behavior rule form");
                return StatusCode(500, "Có lỗi xảy ra khi tải form");
            }
        }

        #endregion
    }

    public class SaveRuleRequest
    {
        public string? Content { get; set; }
    }

    public class SaveAppBriefHtmlRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class SaveAppBriefPdfRequest
    {
        public string TempFilePath { get; set; } = string.Empty;
    }

    public class SaveBehaviorRulesFileRequest
    {
        public string TempFilePath { get; set; } = string.Empty;
        public string? GroupId { get; set; }
    }
}

