using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Groups
{
    public class UpdateGroupRequest
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên hội nhóm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên hội nhóm không được vượt quá 100 ký tự")]
        public string GroupName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Rule { get; set; }

        public bool IsActive { get; set; } = true;
        public IFormFile? Logo { get; set; }
        public string? MainActivities { get; set; } // Các hoạt động chính
    }
}
