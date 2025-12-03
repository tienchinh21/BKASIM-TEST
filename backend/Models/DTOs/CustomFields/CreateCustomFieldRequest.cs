using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request model for creating a new custom field
    /// </summary>
    public class CreateCustomFieldRequest
    {
        /// <summary>
        /// The ID of the parent tab (nullable for flat fields)
        /// </summary>
        public string? CustomFieldTabId { get; set; }

        /// <summary>
        /// The type of entity this field belongs to
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the entity instance
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the field
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The data type of the field
        /// </summary>
        public EEventFieldType FieldType { get; set; }

        /// <summary>
        /// Options for Dropdown or MultipleChoice fields (JSON string)
        /// </summary>
        public string? FieldOptions { get; set; }

        /// <summary>
        /// /// Whether this field is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The display order of this field
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
