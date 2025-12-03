    using MiniAppGIBA.Models.DTOs.Commons;
using MiniAppGIBA.Base.Interface;
using Microsoft.AspNetCore.Http;
namespace MiniAppGIBA.Services.Commons
{
    public class AppBriefService : IAppBriefService
    {
        private readonly ISystemConfigService _systemConfigService;
        private readonly ILogger<AppBriefService> _logger;
        private readonly IUrl _url;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string APP_BRIEF_TYPE = "AppBrief";
        private const string UPLOAD_DIR = "uploads/app_brief";
        private const long MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

        public AppBriefService(
            ISystemConfigService systemConfigService,
            ILogger<AppBriefService> logger,
            IUrl url,
            IHttpContextAccessor httpContextAccessor)
        {
            _systemConfigService = systemConfigService;
            _logger = logger;
            _url = url;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AppBriefDTO?> GetAppBriefAsync()
        {
            try
            {
                var config = await _systemConfigService.GetByTypeAsync(APP_BRIEF_TYPE);
                
                if (config == null || string.IsNullOrEmpty(config.Content))
                {
                    return new AppBriefDTO
                    {
                        Content = string.Empty,
                        IsPdf = false
                    };
                }
                var fullUrl = await _url.ToFullUrl(config.Content,_httpContextAccessor.HttpContext);
                Console.WriteLine("fullUrl: " + fullUrl);
                var isPdf = config.Content.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase);
                
                return new AppBriefDTO
                {
                    Content = config.Content,
                    IsPdf = isPdf,
                    PdfUrl = isPdf ? config.Content : null,
                    PdfFileName = isPdf ? Path.GetFileName(config.Content) : null,
                            FullUrl = isPdf ? fullUrl ??null : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting App Brief");
                return null;
            }
        }

        public async Task<bool> SaveHtmlContentAsync(string htmlContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    throw new ArgumentException("HTML content cannot be empty", nameof(htmlContent));
                }

                // Get existing config to check if there's a PDF file to delete
                var existing = await _systemConfigService.GetByTypeAsync(APP_BRIEF_TYPE);
                if (existing != null && !string.IsNullOrEmpty(existing.Content) && 
                    existing.Content.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    // Note: PDF file deletion should be handled by controller with webRootPath
                    // We just update the content here
                }

                await _systemConfigService.UpsertByTypeAsync(APP_BRIEF_TYPE, htmlContent);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving HTML content");
                throw;
            }
        }

        public async Task<AppBriefDTO> UploadPdfTempAsync(IFormFile file, string webRootPath)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is required", nameof(file));
                }

                if (file.Length > MAX_FILE_SIZE)
                {
                    throw new ArgumentException($"File size exceeds maximum limit of {MAX_FILE_SIZE / (1024 * 1024)}MB");
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    throw new ArgumentException("Only PDF files are allowed");
                }

                // Create temp directory
                var tempDir = Path.Combine(webRootPath, UPLOAD_DIR, "temp");
                Directory.CreateDirectory(tempDir);

                // Generate unique filename
                var safeName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("(", "")
                    .Replace(")", "");
                var fileName = $"{safeName}_{Guid.NewGuid():N}.pdf";
                var filePath = Path.Combine(tempDir, fileName);

                // Save file to temp directory (không lưu vào DB)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return temp path for preview
                var tempRelativePath = $"/{UPLOAD_DIR}/temp/{fileName}";
                return new AppBriefDTO
                {
                    Content = tempRelativePath,
                    IsPdf = true,
                    PdfUrl = tempRelativePath,
                    PdfFileName = fileName
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<AppBriefDTO> SavePdfFromTempAsync(string tempFilePath, string webRootPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tempFilePath))
                {
                    throw new ArgumentException("Temp file path is required", nameof(tempFilePath));
                }

                // Get full temp file path
                var fullTempPath = Path.Combine(webRootPath, tempFilePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullTempPath))
                {
                    throw new FileNotFoundException("Temp file not found");
                }

                // Create upload directory
                var uploadDir = Path.Combine(webRootPath, UPLOAD_DIR);
                Directory.CreateDirectory(uploadDir);

                // Get filename from temp path
                var fileName = Path.GetFileName(tempFilePath);
                var finalFilePath = Path.Combine(uploadDir, fileName);

                // Move file from temp to final location
                System.IO.File.Move(fullTempPath, finalFilePath, true);

                // Get existing config to delete old PDF if exists
                var existing = await _systemConfigService.GetByTypeAsync(APP_BRIEF_TYPE);
                if (existing != null && !string.IsNullOrEmpty(existing.Content) && 
                    existing.Content.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
                    !existing.Content.Contains("/temp/"))
                {
                    // Delete old PDF file (không xóa file temp)
                    var oldFilePath = Path.Combine(webRootPath, existing.Content.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch
                        {
                            // Ignore delete errors
                        }
                    }
                }

                // Save relative path to database
                var relativePath = $"/{UPLOAD_DIR}/{fileName}";
                await _systemConfigService.UpsertByTypeAsync(APP_BRIEF_TYPE, relativePath);

                return new AppBriefDTO
                {
                    Content = relativePath,
                    IsPdf = true,
                    PdfUrl = relativePath,
                    PdfFileName = fileName
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeletePdfAsync(string webRootPath)
        {
            try
            {
                var config = await _systemConfigService.GetByTypeAsync(APP_BRIEF_TYPE);
                
                if (config == null || string.IsNullOrEmpty(config.Content))
                {
                    return true; // Nothing to delete
                }

                // Check if it's a PDF file
                if (config.Content.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    // Delete physical file (không xóa file temp)
                    if (!config.Content.Contains("/temp/"))
                    {
                        var filePath = Path.Combine(webRootPath, config.Content.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                            }
                            catch
                            {
                                // Ignore delete errors
                            }
                        }
                    }

                    // Clear content in database
                    await _systemConfigService.UpsertByTypeAsync(APP_BRIEF_TYPE, string.Empty);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PDF");
                throw;
            }
        }
    }
}

