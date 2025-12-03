using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service interface for managing custom field values
    /// </summary>
    public interface ICustomFieldValueService
    {
        /// <summary>
        /// Retrieves all values for a specific entity, organized by tabs
        /// </summary>
        Task<List<CustomFieldValueDTO>> GetValuesByEntityAsync(ECustomFieldEntityType entityType, string entityId);

        /// <summary>
        /// Stores submitted field values for an entity
        /// </summary>
        Task<List<CustomFieldValueDTO>> CreateValuesAsync(CreateCustomFieldValuesRequest request);

        /// <summary>
        /// Removes a single custom field value
        /// </summary>
        Task<bool> DeleteValueAsync(string valueId);

        /// <summary>
        /// Retrieves all values for a specific field
        /// </summary>
        Task<List<CustomFieldValueDTO>> GetValuesByFieldAsync(string fieldId);
    }
}
