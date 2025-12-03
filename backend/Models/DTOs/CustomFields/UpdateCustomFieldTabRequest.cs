namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request model for updating an existing custom field tab
    /// </summary>
    public class UpdateCustomFieldTabRequest
    {
        /// <summary>
        /// The ID of the tab to update
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The new name of the tab
        /// </summary>
        public string TabName { get; set; } = string.Empty;

        /// <summary>
        /// The new display order of this tab
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
