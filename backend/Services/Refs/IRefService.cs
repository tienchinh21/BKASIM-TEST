using MiniAppGIBA.Models.DTOs.Refs;
using MiniAppGIBA.Models.Request.Refs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Refs;

namespace MiniAppGIBA.Services.Refs
{
    public interface IRefService
    {
        Task<RefDTO> CreateRefAsync(string userZaloId, CreateRefRequest request);
        Task<PagedResult<RefDTO>> GetMyRefsAsync(string userZaloId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? type = null);
        Task<RefDTO> UpdateRefValueAsync(string refId, UpdateRefValueRequest request);
        Task<RefDTO?> GetRefByIdAsync(string refId);
        Task<PagedResult<RefDTO>> GetRefsForCMSAsync(RefQueryParameters queryParameters, List<string>? allowedGroupIds = null);
        Task<List<string>> GetUserGroupIdsAsync(string userZaloId);
    }
}
