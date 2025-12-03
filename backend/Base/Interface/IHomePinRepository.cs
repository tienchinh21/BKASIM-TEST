using MiniAppGIBA.Entities.HomePins;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Base.Interface
{
    /// <summary>
    /// Repository interface for HomePin entity operations
    /// </summary>
    public interface IHomePinRepository
    {
        /// <summary>
        /// Get a pin by its ID
        /// </summary>
        Task<HomePin?> GetByIdAsync(string id);

        /// <summary>
        /// Get a pin by entity type and entity ID
        /// </summary>
        Task<HomePin?> GetByEntityAsync(PinEntityType entityType, string entityId);

        /// <summary>
        /// Get all active pins ordered by display order
        /// </summary>
        Task<List<HomePin>> GetAllActiveAsync();

        /// <summary>
        /// Get pins filtered by entity type
        /// </summary>
        Task<List<HomePin>> GetByEntityTypeAsync(PinEntityType entityType);

        /// <summary>
        /// Get count of active pins
        /// </summary>
        Task<int> GetActivePinsCountAsync();

        /// <summary>
        /// Add a new pin
        /// </summary>
        Task<HomePin> AddAsync(HomePin pin);

        /// <summary>
        /// Update an existing pin
        /// </summary>
        Task UpdateAsync(HomePin pin);

        /// <summary>
        /// Delete a pin by ID
        /// </summary>
        Task DeleteAsync(string id);

        /// <summary>
        /// Check if a pin exists for the given entity
        /// </summary>
        Task<bool> ExistsAsync(PinEntityType entityType, string entityId);

        /// <summary>
        /// Get the maximum display order value
        /// </summary>
        Task<int> GetMaxDisplayOrderAsync();

        /// <summary>
        /// Reorder multiple pins in a single transaction
        /// </summary>
        Task ReorderPinsAsync(List<HomePin> pins);
    }
}
