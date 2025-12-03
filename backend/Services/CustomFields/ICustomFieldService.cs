using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service interface for managing custom fields
    /// </summary>
    public interface ICustomFieldService
    {
        /// <summary>
        /// Retrieves all fields for a specific tab, ordered by display order
        /// </summary>
        Task<List<CustomFieldDTO>> GetFieldsByTabAsync(string tabId);

        /// <summary>
        /// /// Retrieves all fields for a specific entity, optionally filtered by tab
        /// </summary>
        Task<List<CustomFieldDTO>> GetFieldsByEntityAsync(ECustomFieldEntityType entityType, string entityId, string? tabId = null);

        /// <summary>
        /// Creates a new field with validation for required fields and field type support
        /// </summary>
        Task<CustomFieldDTO> CreateFieldAsync(CreateCustomFieldRequest request);

        /// <summary>
        /// /// Updates an existing field with round-trip persistence for all properties
        /// /// </summary>
        Task<CustomFieldDTO> UpdateFieldAsync(UpdateCustomFieldRequest request);

        /// <summary>
        /// Deletes a field and archives related submitted values with field name
        /// </summary>
        Task<bool> DeleteFieldAsync(string fieldId);

        /// <summary>
        /// Reorders fields within a tab by updating their display order
        /// </summary>
        Task<bool> ReorderFieldsAsync(string tabId, List<(string FieldId, int Order)> reordering);
    }
}
