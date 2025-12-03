namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// DTO for group custom field form structure
    /// </summary>
    public class GroupCustomFieldFormDTO
    {
        /// <summary>
        /// The ID of the group
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the group
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// List of tabs with their fields, ordered by DisplayOrder
        /// </summary>
        public List<CustomFieldTabWithFieldsDTO> Tabs { get; set; } = new();
    }
}
