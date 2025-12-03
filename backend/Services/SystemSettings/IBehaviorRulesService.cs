using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Request.SystemSettings;
using MiniAppGIBA.Models.Response.SystemSettings;

namespace MiniAppGIBA.Services.SystemSettings
{
    public interface IBehaviorRulesService
    {
        /// <summary>
        /// Lấy quy tắc ứng xử theo loại và group
        /// </summary>
        Task<string> GetBehaviorRulesAsync(EBehaviorRuleType type, string? groupId = null);

        /// <summary>
        /// Lấy thông tin chi tiết quy tắc ứng xử bao gồm metadata
        /// </summary>
        Task<BehaviorRulesResponse> GetBehaviorRulesDetailAsync(EBehaviorRuleType type, string? groupId = null);

        /// <summary>
        /// Thay thế file quy tắc ứng xử (xóa cũ, upload mới)
        /// </summary>
        Task<BehaviorRulesFileResult> ReplaceFileAsync(BehaviorRulesUploadRequest request);

        /// <summary>
        /// Upload file tạm thời (chỉ để preview, chưa lưu vào DB)
        /// </summary>
        Task<BehaviorRulesFileResult> UploadFileTempAsync(IFormFile file, string webRootPath, string? groupId = null);

        /// <summary>
        /// Lưu file từ temp vào DB (sau khi user bấm nút Lưu)
        /// </summary>
        Task<BehaviorRulesFileResult> SaveFileFromTempAsync(string tempFilePath, string webRootPath, string? groupId = null);

        /// <summary>
        /// Lưu URL quy tắc ứng xử (legacy method)
        /// </summary>
        Task SaveBehaviorRulesUrlAsync(string url);

        /// <summary>
        /// Xóa quy tắc ứng xử của group
        /// </summary>
        Task<BehaviorRulesFileResult> DeleteBehaviorRulesForGroupAsync(string groupId, string webRootPath);
    }
}
