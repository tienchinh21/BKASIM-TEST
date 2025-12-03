using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Groups
{
    public class JoinGroupRequest
    {
        [Required(ErrorMessage = "GroupId là bắt buộc")]
        public string GroupId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do gia nhập là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Lý do gia nhập không được quá 200 ký tự")]
        public string Reason { get; set; } = string.Empty;

        public string? Company { get; set; }
        public string? Position { get; set; }

        /// <summary>
        /// Dictionary of custom field IDs to submitted values
        /// Key: CustomFieldId, Value: submitted field value
        /// </summary>
        public Dictionary<string, string>? CustomFieldValues { get; set; }
    }
}
