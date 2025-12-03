using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.HomePins;

namespace MiniAppGIBA.Services.HomePins
{
    public interface IHomePinService
    {
        Task<Result<HomePinDto>> PinEntityAsync(CreateHomePinRequest request, string adminId);
        Task<Result<bool>> UnpinEntityAsync(string pinId, string adminId);
        Task<Result<HomePinListResponse>> GetHomePinsAsync(PinEntityType? filterType = null); // For admin - no privacy filtering
        Task<Result<HomePinListResponse>> GetHomePinsForUserAsync(string? userZaloId, PinEntityType? filterType = null); // For users - with privacy filtering
        Task<Result<bool>> ReorderPinsAsync(ReorderPinsRequest request, string adminId);
        Task<Result<HomePinDto>> UpdatePinNotesAsync(string pinId, string notes, string adminId);
        Task<Result<bool>> ValidateEntityExistsAsync(PinEntityType entityType, string entityId);
        Task<Result<List<object>>> GetAvailableEntitiesForAdminAsync(PinEntityType entityType); // For admin dropdown - get ALL entities
    }
}
