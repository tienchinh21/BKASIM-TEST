namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// DTO for custom field tab with submitted values
    /// </summary>
    public class CustomFieldTabWithValuesDTO
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
        /// List of fields with their values
        /// </summary>
        public List<CustomFieldWithValueDTO> Fields { get; set; } = new();
    }
}
