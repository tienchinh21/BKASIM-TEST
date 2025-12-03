using MiniAppGIBA.Models.DTOs.Events;

namespace MiniAppGIBA.Models.Response.Events
{
    // Kế thừa ComingSoonEventDTO để trả về full chi tiết sự kiện
    // (bao gồm eventGifts, eventSponsors, images dạng list, ...)
    public class EventGuestByPhoneResponse : ComingSoonEventDTO
    {
        public string? GuestName { get; set; }
        public string? UserZaloId { get; set; }
        public string? Avatar { get; set; }
        public string? GuestListId { get; set; }
        public int? FormStatus { get; set; }
        public bool? CheckInStatus { get; set; }
    }
}