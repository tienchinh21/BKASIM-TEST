using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn hội nhóm")]
        public string GroupId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên sự kiện")]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime EndTime { get; set; }

        [Range(1, 2, ErrorMessage = "Loại sự kiện không hợp lệ")]
        public byte Type { get; set; } = 1;

        [Range(-1, int.MaxValue, ErrorMessage = "Giới hạn số người phải >= -1")]
        public int JoinCount { get; set; } = -1; // -1 = không giới hạn, > 0 = giới hạn số người

        public string? MeetingLink { get; set; }
        public string? GoogleMapURL { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public bool NeedApproval { get; set; } = false;

        // File uploads
        public IFormFile? BannerFile { get; set; }
        public IFormFile? ImageFiles { get; set; } // Changed to single file

        // Gift data
        public string? GiftsData { get; set; } // JSON string of gifts
        public IFormFile? GiftImages { get; set; } // Gift image file (single file for now)

        // Sponsor data
        public string? SponsorId { get; set; }
        public string? SponsorshipTierId { get; set; }

        // Custom fields data
        public string? CustomFieldsData { get; set; } // JSON string of custom fields
    }

    public class UpdateEventRequest : CreateEventRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
    }
}
