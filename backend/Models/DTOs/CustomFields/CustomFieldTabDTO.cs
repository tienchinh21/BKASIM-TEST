namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Data transfer object for custom field tab
    /// </summary>
    public class CustomFieldTabDTO
    {
        /// <summary>
        /// Unique identifier for the tab
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity this tab belongs to (e.g., GroupId)
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the tab
        /// </summary>
        public string TabName { get; set; } = string.Empty;

        /// <summary>
        /// The display order of this tab
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Number of fields in this tab
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// Date when the tab was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when the tab was last updated
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
