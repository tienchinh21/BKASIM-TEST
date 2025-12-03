using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Data transfer object for custom field
    /// </summary>
    public class CustomFieldDTO
    {
        /// <summary>
        /// Unique identifier for the field
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the parent tab (nullable for flat fields)
        /// </summary>
        public string? CustomFieldTabId { get; set; }

        /// <summary>
        /// The ID of the entity this field belongs to
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
        /// Text representation of the field type
        /// </summary>
        public string FieldTypeText { get; set; } = string.Empty;

        /// <summary>
        /// Options for Dropdown or MultipleChoice fields
        /// </summary>
        public List<string>? FieldOptions { get; set; }

        /// <summary>
        /// Whether this field is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The display order of this field
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Date when the field was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when the field was last updated
        /// /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
