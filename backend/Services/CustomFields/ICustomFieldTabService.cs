using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service interface for managing custom field tabs
    /// </summary>
    public interface ICustomFieldTabService
    {
        /// <summary>
        /// Retrieves all tabs for a specific entity, ordered by display order
        /// </summary>
        Task<List<CustomFieldTabDTO>> GetTabsByEntityAsync(ECustomFieldEntityType entityType, string entityId);

        /// <summary>
        /// Creates a new tab with validation for required fields
        /// </summary>
        Task<CustomFieldTabDTO> CreateTabAsync(CreateCustomFieldTabRequest request);

        /// <summary>
        /// Updates an existing tab with round-trip persistence
        /// </summary>
        Task<CustomFieldTabDTO> UpdateTabAsync(UpdateCustomFieldTabRequest request);

        /// <summary>
        /// Deletes a tab and all associated custom fields (cascade deletion)
        /// </summary>
        Task<bool> DeleteTabAsync(string tabId);

        /// <summary>
        /// Reorders tabs by updating their display order
        /// </summary>
        Task<bool> ReorderTabsAsync(string entityId, List<(string TabId, int Order)> reordering);
    }
}
