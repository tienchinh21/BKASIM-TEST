using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Events
{
    public class GuestList : BaseEntity
    {
        public string EventGuestId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public byte Status { get; set; } = 1; // 1 - Chờ xử lý, 2 - Đã duyệt, 3 - Từ chối, 4 - Hủy
        public string? CheckInCode { get; set; }
        public DateTime? CheckInTime { get; set; }
        public bool? CheckInStatus { get; set; } = false ;

        // Navigation properties
        public virtual EventGuest EventGuest { get; set; } = null!;
    }
}
