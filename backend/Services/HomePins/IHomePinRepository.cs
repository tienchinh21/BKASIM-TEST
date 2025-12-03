using MiniAppGIBA.Entities.HomePins;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Services.HomePins
{
    /// <summary>
    /// Repository interface for HomePin entity
    /// </summary>
    public interface IHomePinRepository
    {
        Task<HomePin?> GetByIdAsync(string id);
        Task<HomePin?> GetByEntityAsync(PinEntityType entityType, string entityId);
        Task<List<HomePin>> GetAllActiveAsync();
        Task<List<HomePin>> GetByEntityTypeAsync(PinEntityType entityType);
        Task<int> GetActivePinsCountAsync();
        Task<HomePin> AddAsync(HomePin pin);
        Task UpdateAsync(HomePin pin);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(PinEntityType entityType, string entityId);
        Task<int> GetMaxDisplayOrderAsync();
        Task ReorderPinsAsync(List<HomePin> pins);
    }
}
