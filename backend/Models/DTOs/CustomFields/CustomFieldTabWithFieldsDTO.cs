namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// DTO for custom field tab with its fields
    /// </summary>
    public class CustomFieldTabWithFieldsDTO
    {
        /// <summary>
        /// Unique identifier for the tab
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The name of the tab
        /// </summary>
        public string TabName { get; set; } = string.Empty;

        /// <summary>
        /// The display order of this tab
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// List of fields in this tab, ordered by DisplayOrder
        /// </summary>
        public List<CustomFieldDTO> Fields { get; set; } = new();
    }
}
