namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request DTO for submitting custom field values
    /// </summary>
    public class SubmitCustomFieldValuesRequest
    {
        /// <summary>
        /// Dictionary of field ID to field value
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new();
    }
}
