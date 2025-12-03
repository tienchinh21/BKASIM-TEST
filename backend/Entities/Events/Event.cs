using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Sponsors;

namespace MiniAppGIBA.Entities.Events
{
    public class Event : BaseEntity
    {
        public string GroupId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Banner { get; set; }
        public string? Images { get; set; } // Comma-separated image URLs
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int JoinCount { get; set; } = 0; // -1 = không giới hạn, > 0 = giới hạn số người
        public byte Type { get; set; } = 1; // 1 - Nội bộ (chỉ hiển thị trong Hội nhóm), 2 - Công khai (hiển thị công khai)
        public string? MeetingLink { get; set; }
        public string? GoogleMapURL { get; set; }
        public string? Address { get; set; }
        public byte Status { get; set; } = 1; // 1 - sắp diễn ra, 2 - đang diễn ra, 3 - đã kết thúc (dựa trên thời gian bắt đầu - kết thúc)
        public bool IsActive { get; set; } = true;
        public bool NeedApproval { get; set; } = false;

        // Navigation properties
        public virtual Group Group { get; set; } = null!;
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public virtual ICollection<EventGuest> EventGuests { get; set; } = new List<EventGuest>();
        public virtual ICollection<EventGift> EventGifts { get; set; } = new List<EventGift>();
        public virtual ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}