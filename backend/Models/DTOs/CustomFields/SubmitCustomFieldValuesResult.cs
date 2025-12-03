namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Result DTO for custom field values submission
    /// </summary>
    public class SubmitCustomFieldValuesResult
    {
        /// <summary>
        /// Whether the submission was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The ID of the membership group
        /// </summary>
        public string MembershipGroupId { get; set; } = string.Empty;

        /// <summary>
        /// Whether custom fields have been submitted for this membership group
        /// </summary>
        public bool HasCustomFieldsSubmitted { get; set; }

        /// <summary>
        /// List of submitted values (populated on success)
        /// </summary>
        public List<CustomFieldValueDTO> SubmittedValues { get; set; } = new();

        /// <summary>
        /// Dictionary of field ID to error message (populated on validation failure)
        /// </summary>
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}
