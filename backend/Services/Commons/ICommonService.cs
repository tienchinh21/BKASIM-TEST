using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Services.Commons
{
    public interface ISystemConfigService : IService<SystemConfig>
    {
        /// <summary>
        /// Lấy nội dung theo Type
        /// </summary>
        Task<SystemConfig?> GetByTypeAsync(string type);

        /// <summary>
        /// Cập nhật hoặc tạo mới nội dung theo Type
        /// </summary>
        Task<int> UpsertByTypeAsync(string type, string content);

        /// <summary>
        /// Lấy tất cả nội dung theo Type (phân trang)
        /// </summary>
        Task<PagedResult<SystemConfig>> GetByTypePagedAsync(string type, IRequestQuery query);


    /// <summary>
    /// Lấy nội dung quy tắc ứng xử
    /// </summary>
    Task<string> GetBehaviorRulesFileAsync(EBehaviorRuleType type, string? groupId);
    }
}

