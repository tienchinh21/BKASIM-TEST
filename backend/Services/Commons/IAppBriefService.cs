using MiniAppGIBA.Models.DTOs.Commons;

namespace MiniAppGIBA.Services.Commons
{
    public interface IAppBriefService
    {
        /// <summary>
        /// Lấy nội dung App Brief (HTML hoặc PDF path)
        /// </summary>
        Task<AppBriefDTO?> GetAppBriefAsync();

        /// <summary>
        /// Lưu nội dung HTML vào App Brief
        /// </summary>
        Task<bool> SaveHtmlContentAsync(string htmlContent);

        /// <summary>
        /// Upload file PDF tạm thời (chỉ để preview, chưa lưu vào DB)
        /// </summary>
        Task<AppBriefDTO> UploadPdfTempAsync(IFormFile file, string webRootPath);

        /// <summary>
        /// Lưu file PDF từ temp vào DB (sau khi user bấm nút Lưu)
        /// </summary>
        Task<AppBriefDTO> SavePdfFromTempAsync(string tempFilePath, string webRootPath);

        /// <summary>
        /// Xóa file PDF cũ (nếu có) và reset về HTML
        /// </summary>
        Task<bool> DeletePdfAsync(string webRootPath);
    }
}

