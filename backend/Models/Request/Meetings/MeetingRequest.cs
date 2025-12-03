
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.Request.Meetings
{
    public class MeetingRequest
    {
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
        public string? CreatedBy { get; set; } = string.Empty;
        public string? RoleName { get; set; } = string.Empty;
    }
}

