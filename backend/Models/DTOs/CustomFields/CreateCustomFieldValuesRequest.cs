using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request model for creating custom field values
    /// </summary>
    public class CreateCustomFieldValuesRequest
    {
        /// <summary>
        /// The type of entity these values belong to
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the entity instance
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of field IDs to submitted values
        /// </summary>
        public Dictionary<string, string> FieldValues { get; set; } = new();
    }
}
/// 