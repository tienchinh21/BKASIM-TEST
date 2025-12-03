namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Data transfer object for custom field value
    /// </summary>
    public class CustomFieldValueDTO
    {
        /// <summary>
        /// Unique identifier for the value
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the custom field definition
        /// </summary>
        public string CustomFieldId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity instance
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the field (archived field name for deleted fields)
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The actual value submitted by the user
        /// </summary>
        public string FieldValue { get; set; } = string.Empty;

        /// <summary>
        /// Date when the value was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when the value was last updated
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
