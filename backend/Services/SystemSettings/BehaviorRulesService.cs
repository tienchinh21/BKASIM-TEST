using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Request.SystemSettings;
using MiniAppGIBA.Models.Response.SystemSettings;
using MiniAppGIBA.Services.Commons;

namespace MiniAppGIBA.Services.SystemSettings
{
    public class BehaviorRulesService(IUnitOfWork unitOfWork, ISystemConfigService systemConfigService, ILogger<BehaviorRulesService> logger) : IBehaviorRulesService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRepository<SystemConfig> _systemConfigRepository = unitOfWork.GetRepository<SystemConfig>();
        private readonly IRepository<Group> _groupRepository = unitOfWork.GetRepository<Group>();

        public async Task<string> GetBehaviorRulesAsync(EBehaviorRuleType type, string? groupId = null)
        {
            try
            {
                var result = "";
                
                if (type == EBehaviorRuleType.BehaviorRulesSupperAdmin)
                {
                    var rule = await systemConfigService.GetByTypeAsync("BehaviorRules");
                    result = rule?.Content ?? "";
                }
                else
                {
                    var group = await _groupRepository.AsQueryable()
                        .FirstOrDefaultAsync(g => g.Id == groupId);
                    
                    if (group != null && !string.IsNullOrEmpty(group.BehaviorRulesUrl))
                    {
                        // Group.BehaviorRulesUrl now contains direct file path
                        result = group.BehaviorRulesUrl;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting behavior rules for type: {Type}, groupId: {GroupId}", type, groupId);
                return "";
            }
        }

        public async Task<BehaviorRulesResponse> GetBehaviorRulesDetailAsync(EBehaviorRuleType type, string? groupId = null)
        {
            try
            {
                if (type == EBehaviorRuleType.BehaviorRulesSupperAdmin)
                {
                    var rule = await systemConfigService.GetByTypeAsync("BehaviorRules");
                    
                    if (rule != null && !string.IsNullOrEmpty(rule.Content))
                    {
                        return new BehaviorRulesResponse
                        {
                            Success = true,
                            Data = new BehaviorRulesData
                            {
                                FileUrl = rule.Content,
                                FileName = GetFileNameFromUrl(rule.Content),
                                CreatedDate = rule.CreatedDate,
                                UpdatedDate = rule.UpdatedDate,
                                IsActive = true
                            }
                        };
                    }
                }
                else
                {
                    var group = await _groupRepository.AsQueryable()
                        .FirstOrDefaultAsync(g => g.Id == groupId);
                    
                    if (group != null && !string.IsNullOrEmpty(group.BehaviorRulesUrl))
                    {
                        // Group.BehaviorRulesUrl now contains direct file path
                        return new BehaviorRulesResponse
                        {
                            Success = true,
                            Data = new BehaviorRulesData
                            {
                                FileUrl = group.BehaviorRulesUrl,
                                FileName = GetFileNameFromUrl(group.BehaviorRulesUrl),
                                CreatedDate = group.CreatedDate,
                                UpdatedDate = group.UpdatedDate,
                                IsActive = true
                            }
                        };
                    }
                }

                return new BehaviorRulesResponse
                {
                    Success = false,
                    Message = "Chưa có quy tắc ứng xử được cấu hình"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting behavior rules detail for type: {Type}, groupId: {GroupId}", type, groupId);
                return new BehaviorRulesResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin quy tắc ứng xử"
                };
            }
        }

        public async Task<BehaviorRulesFileResult> ReplaceFileAsync(BehaviorRulesUploadRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "Vui lòng chọn file để tải lên"
                    };
                }

                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(request.File.FileName);
                
                if (!allowedExt.Contains(ext))
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = $"Định dạng file không hỗ trợ: {ext}"
                    };
                }

                if (request.File.Length > 50 * 1024 * 1024)
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "File quá lớn. Vui lòng chọn file nhỏ hơn 50MB"
                    };
                }

                string uploadDir;
                string relativeDir;
                
                if (!string.IsNullOrEmpty(request.GroupId))
                {
                    // Upload for specific group
                    uploadDir = Path.Combine(request.WebRootPath, "uploads", "behavior_rules", "groups", request.GroupId);
                    relativeDir = $"/uploads/behavior_rules/groups/{request.GroupId}";
                }
                else if (request.IsSuperAdmin)
                {
                    uploadDir = Path.Combine(request.WebRootPath, "uploads", "behavior_rules", "superadmin");
                    relativeDir = "/uploads/behavior_rules/superadmin";
                }
                else
                {
                    var groupName = request.AdminGroups?.FirstOrDefault() ?? "default";
                    uploadDir = Path.Combine(request.WebRootPath, "uploads", "behavior_rules", "admin", groupName);
                    relativeDir = $"/uploads/behavior_rules/admin/{groupName}";
                }

                Directory.CreateDirectory(uploadDir);

                if (Directory.Exists(uploadDir))
                {
                    var oldFiles = Directory.GetFiles(uploadDir);
                    foreach (var oldFile in oldFiles)
                    {
                        try { System.IO.File.Delete(oldFile); } catch { }
                    }
                }

                var safeName = Path.GetFileNameWithoutExtension(request.File.FileName);
                var fileName = $"{safeName}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                var fileUrl = $"{relativeDir}/{fileName}";

                // Save to appropriate location
                if (!string.IsNullOrEmpty(request.GroupId))
                {
                    // Save to Group.BehaviorRulesUrl
                    var group = await _groupRepository.AsQueryable()
                        .FirstOrDefaultAsync(g => g.Id == request.GroupId);
                    
                    if (group != null)
                    {
                        group.BehaviorRulesUrl = fileUrl;
                        _groupRepository.Update(group);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        return new BehaviorRulesFileResult
                        {
                            Success = false,
                            Message = "Không tìm thấy nhóm"
                        };
                    }
                }
                else
                {
                    // Save to SystemConfig for super admin
                    await systemConfigService.UpsertByTypeAsync("BehaviorRules", fileUrl);
                }

                return new BehaviorRulesFileResult
                {
                    Success = true,
                    Message = "Thay đổi tài liệu thành công",
                    FileUrl = fileUrl,
                    FileName = request.File.FileName,
                    FileSize = FormatFileSize(request.File.Length)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error replacing behavior rules file");
                return new BehaviorRulesFileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi thay đổi tài liệu"
                };
            }
        }

        public async Task<BehaviorRulesFileResult> UploadFileTempAsync(IFormFile file, string webRootPath, string? groupId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "Vui lòng chọn file để tải lên"
                    };
                }

                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(file.FileName);
                
                if (!allowedExt.Contains(ext))
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = $"Định dạng file không hỗ trợ: {ext}"
                    };
                }

                if (file.Length > 50 * 1024 * 1024)
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "File quá lớn. Vui lòng chọn file nhỏ hơn 50MB"
                    };
                }

                // Create temp directory
                string tempDir;
                string tempRelativeDir;
                
                if (!string.IsNullOrEmpty(groupId))
                {
                    tempDir = Path.Combine(webRootPath, "uploads", "behavior_rules", "temp", "groups", groupId);
                    tempRelativeDir = $"/uploads/behavior_rules/temp/groups/{groupId}";
                }
                else
                {
                    tempDir = Path.Combine(webRootPath, "uploads", "behavior_rules", "temp", "superadmin");
                    tempRelativeDir = "/uploads/behavior_rules/temp/superadmin";
                }

                Directory.CreateDirectory(tempDir);

                var safeName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("(", "")
                    .Replace(")", "");
                var fileName = $"{safeName}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(tempDir, fileName);

                // Save file to temp directory
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var tempFileUrl = $"{tempRelativeDir}/{fileName}";

                return new BehaviorRulesFileResult
                {
                    Success = true,
                    Message = "Tải lên file thành công. Vui lòng bấm 'Lưu' để lưu vào hệ thống.",
                    FileUrl = tempFileUrl,
                    FileName = file.FileName,
                    FileSize = FormatFileSize(file.Length)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading temp file");
                return new BehaviorRulesFileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tải lên file"
                };
            }
        }

        public async Task<BehaviorRulesFileResult> SaveFileFromTempAsync(string tempFilePath, string webRootPath, string? groupId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tempFilePath))
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "Đường dẫn file không hợp lệ"
                    };
                }

                // Get full temp file path
                var fullTempPath = Path.Combine(webRootPath, tempFilePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullTempPath))
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "File không tồn tại. Vui lòng upload lại."
                    };
                }

                // Determine final upload directory
                string uploadDir;
                string relativeDir;
                
                if (!string.IsNullOrEmpty(groupId))
                {
                    uploadDir = Path.Combine(webRootPath, "uploads", "behavior_rules", "groups", groupId);
                    relativeDir = $"/uploads/behavior_rules/groups/{groupId}";
                }
                else
                {
                    uploadDir = Path.Combine(webRootPath, "uploads", "behavior_rules", "superadmin");
                    relativeDir = "/uploads/behavior_rules/superadmin";
                }

                Directory.CreateDirectory(uploadDir);

                // Delete old files
                if (Directory.Exists(uploadDir))
                {
                    var oldFiles = Directory.GetFiles(uploadDir);
                    foreach (var oldFile in oldFiles)
                    {
                        try { System.IO.File.Delete(oldFile); } catch { }
                    }
                }

                // Get filename from temp path
                var fileName = Path.GetFileName(tempFilePath);
                var finalFilePath = Path.Combine(uploadDir, fileName);

                // Move file from temp to final location
                System.IO.File.Move(fullTempPath, finalFilePath, true);

                var fileUrl = $"{relativeDir}/{fileName}";

                // Save to appropriate location
                if (!string.IsNullOrEmpty(groupId))
                {
                    // Save to Group.BehaviorRulesUrl
                    var group = await _groupRepository.AsQueryable()
                        .FirstOrDefaultAsync(g => g.Id == groupId);
                    
                    if (group != null)
                    {
                        group.BehaviorRulesUrl = fileUrl;
                        _groupRepository.Update(group);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        return new BehaviorRulesFileResult
                        {
                            Success = false,
                            Message = "Không tìm thấy nhóm"
                        };
                    }
                }
                else
                {
                    // Save to SystemConfig for super admin
                    await systemConfigService.UpsertByTypeAsync("BehaviorRules", fileUrl);
                }

                return new BehaviorRulesFileResult
                {
                    Success = true,
                    Message = "Lưu file thành công",
                    FileUrl = fileUrl,
                    FileName = fileName,
                    FileSize = FormatFileSize(new FileInfo(finalFilePath).Length)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving file from temp");
                return new BehaviorRulesFileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lưu file"
                };
            }
        }

        public async Task SaveBehaviorRulesUrlAsync(string url)
        {
            try
            {
                await systemConfigService.UpsertByTypeAsync("BehaviorRules", url);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving behavior rules URL: {Url}", url);
                throw;
            }
        }

        public async Task<BehaviorRulesFileResult> DeleteBehaviorRulesForGroupAsync(string groupId, string webRootPath)
        {
            try
            {
                var group = await _groupRepository.AsQueryable()
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "Không tìm thấy nhóm"
                    };
                }

                if (string.IsNullOrEmpty(group.BehaviorRulesUrl))
                {
                    return new BehaviorRulesFileResult
                    {
                        Success = false,
                        Message = "Nhóm chưa có tài liệu quy tắc ứng xử"
                    };
                }

                // Delete physical file
                try
                {
                    var uploadDir = Path.Combine(webRootPath, "uploads", "behavior_rules", "groups", groupId);
                    if (Directory.Exists(uploadDir))
                    {
                        var files = Directory.GetFiles(uploadDir);
                        foreach (var file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                        Directory.Delete(uploadDir, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error deleting physical files for group {GroupId}", groupId);
                }

                // Clear database reference
                group.BehaviorRulesUrl = null;
                _groupRepository.Update(group);
                await _unitOfWork.SaveChangesAsync();

                return new BehaviorRulesFileResult
                {
                    Success = true,
                    Message = "Đã xóa tài liệu quy tắc ứng xử"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting behavior rules for group {GroupId}", groupId);
                return new BehaviorRulesFileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa tài liệu"
                };
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 Bytes";
            const int k = 1024;
            string[] sizes = { "Bytes", "KB", "MB", "GB" };
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(k));
            return $"{Math.Round(bytes / Math.Pow(k, i), 2)} {sizes[i]}";
        }

        private static string GetFileNameFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return "Tài liệu quy tắc ứng xử";
                
                // Lấy phần cuối sau dấu "/" cuối cùng
                var fileName = url.Split('/').LastOrDefault();
                if (string.IsNullOrEmpty(fileName)) return "Tài liệu quy tắc ứng xử";
                
                // Bỏ query parameters nếu có (?param=value)
                fileName = fileName.Split('?')[0];
                
                // Decode URL encoding (%20 -> space, etc.)
                fileName = Uri.UnescapeDataString(fileName);
                
                // Nếu tên file quá dài, rút ngắn lại
                if (fileName.Length > 50)
                {
                    var ext = Path.GetExtension(fileName);
                    var name = Path.GetFileNameWithoutExtension(fileName);
                    if (name.Length > 45)
                    {
                        name = name.Substring(0, 45);
                    }
                    fileName = name + "..." + ext;
                }
                
                return fileName;
            }
            catch (Exception)
            {
                return "Tài liệu quy tắc ứng xử";
            }
        }
    }
}
