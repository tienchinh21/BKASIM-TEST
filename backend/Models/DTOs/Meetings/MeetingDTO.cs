using MiniAppGIBA.Entities.Meetings;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.Meetings
{
    public class MeetingDTO
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MeetingType MeetingType { get; set; } = MeetingType.Online;
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public bool IsPublic { get; set; } = false;
        public byte Status { get; set; } = 1; // 1: Sắp diễn ra, 2: Đang diễn ra, 3: Đã kết thúc
        public string StatusText { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

