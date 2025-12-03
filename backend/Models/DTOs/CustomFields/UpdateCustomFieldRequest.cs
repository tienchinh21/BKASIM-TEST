using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request model for updating an existing custom field
    /// </summary>
    public class UpdateCustomFieldRequest
    {
        /// <summary>
        /// The ID of the field to update
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The new name of the field
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The new data type of the field
        /// </summary>
        public EEventFieldType FieldType { get; set; }

        /// <summary>
        /// Updated options for Dropdown or MultipleChoice fields (JSON string)
        /// </summary>
        public string? FieldOptions { get; set; }

        /// <summary>
        /// Whether this field is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The new display order of this field
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
