using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Subscriptions;

namespace MiniAppGIBA.Services.Subscriptions
{
    public interface IGroupPackageConfigService
    {
        Task<PagedResult<GroupPackageConfigDTO>> GetPagedAsync(int page, int pageSize, string keyword);
        Task<GroupPackageConfigDTO> CreateAsync(CreateGroupPackageConfigDTO request);
        Task<bool> DeleteAsync(string id);
        Task<bool> ToggleActiveAsync(string id);
        Task<GroupPackageConfigDTO?> GetByIdAsync(string id);
        Task<GroupPackageConfigDTO?> UpdateAsync(string id, UpdateGroupPackageConfigDTO request);
    }
}

