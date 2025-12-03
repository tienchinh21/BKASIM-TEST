namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventDTO
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Banner { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int JoinCount { get; set; }
        public byte Type { get; set; }
        public string TypeText { get; set; } = string.Empty;
        public string? MeetingLink { get; set; }
        public string? GoogleMapURL { get; set; }
        public string? Address { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsCheckIn { get; set; }
        public bool IsRegister { get; set; } = false;
        public bool NeedApproval { get; set; } = false;
        public string? CheckInCode { get; set; }
        public bool IsPinned { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class EventDetailDTO : EventDTO
    {
        public List<EventGiftDTO> Gifts { get; set; } = new List<EventGiftDTO>();
        public List<EventRegistrationDTO> Registrations { get; set; } = new List<EventRegistrationDTO>();
        public List<EventSponsorDTO> Sponsors { get; set; } = new List<EventSponsorDTO>();
    }

	    /// <summary>
	    /// DTO dành riêng cho API ComingSoon, mở rộng EventDTO
	    /// để trả thêm danh sách quà tặng và nhà tài trợ.
	    /// JSON trả về sẽ có: eventGifts, eventSponsors.
	    /// </summary>
	    public class ComingSoonEventDTO : EventDTO
	    {
	        public List<EventGiftDTO> EventGifts { get; set; } = new List<EventGiftDTO>();
	        public List<EventSponsorDTO> EventSponsors { get; set; } = new List<EventSponsorDTO>();
	    }

    public class EventSponsorDTO
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string SponsorshipTierId { get; set; } = string.Empty;
        public string SponsorshipTierName { get; set; } = string.Empty;
    }
}