using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// DTO for custom field with its submitted value
    /// </summary>
    public class CustomFieldWithValueDTO
    {
        /// <summary>
        /// Unique identifier for the field
        /// </summary>
        public string FieldId { get; set; } = string.Empty;

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
        /// Whether this field should be displayed on public profile
        /// </summary>
        public bool IsProfile { get; set; }

        /// <summary>
        /// The submitted value (empty string if no value submitted)
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
