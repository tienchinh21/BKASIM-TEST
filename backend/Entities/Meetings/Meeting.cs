using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.Meetings
{
    public class Meeting : BaseEntity
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public MeetingType MeetingType { get; set; } = MeetingType.Online;
        public string? Location { get; set; } // For offline meetings
        public string? MeetingLink { get; set; } // For online meetings
        public bool IsPublic { get; set; } = false;
        public byte Status { get; set; } = 1; // 1:Sắp diễn ra, 2: Đang diễn ra, 3: Đã hoàn thành, 4: Đã hủy
        public string RoleId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
    }
}

