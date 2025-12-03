namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request DTO for registering to a group with custom field values
    /// </summary>
    public class RegisterGroupWithCustomFieldsRequest
    {
        /// <summary>
        /// Lý do tham gia nhóm
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Tên công ty
        /// </summary>
        public string? Company { get; set; }

        /// <summary>
        /// Chức vụ
        /// </summary>
        public string? Position { get; set; }

        /// <summary>
        /// Dictionary of field ID to field value
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new();
    }
}
