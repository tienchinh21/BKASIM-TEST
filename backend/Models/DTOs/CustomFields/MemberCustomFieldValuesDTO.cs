namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// DTO for member's submitted custom field values
    /// </summary>
    public class MemberCustomFieldValuesDTO
    {
        /// <summary>
        /// The ID of the membership group
        /// </summary>
        public string MembershipGroupId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the group
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Whether custom fields have been submitted
        /// </summary>
        public bool HasCustomFieldsSubmitted { get; set; }

        /// <summary>
        /// List of tabs with their field values
        /// </summary>
        public List<CustomFieldTabWithValuesDTO> Tabs { get; set; } = new();
    }
}
