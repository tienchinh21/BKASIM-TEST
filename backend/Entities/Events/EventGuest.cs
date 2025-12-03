using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Memberships;

namespace MiniAppGIBA.Entities.Events
{
    public class EventGuest : BaseEntity
    {
        public string EventId { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int GuestNumber { get; set; } = 0; // Số lượng khách mời
        public byte Status { get; set; } = 1; // 1 - Chờ xử lý, 2 - Đã duyệt, 3 - Từ chối, 4 - Hủy
        public string? RejectReason { get; set; }
        public string? CancelReason { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual Membership Membership { get; set; } = null!;
        public virtual ICollection<GuestList> GuestLists { get; set; } = new List<GuestList>();
    }
}