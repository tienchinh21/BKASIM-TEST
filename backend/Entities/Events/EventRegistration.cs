using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.Events
{
    public class EventRegistration : BaseEntity
    {
        public string EventId { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CheckInCode { get; set; }
        public int Status { get; set; } = 1; // 1 - pending, 2 - confirmed, 3 - cancelled
        public ECheckInStatus CheckInStatus { get; set; } = ECheckInStatus.NotCheckIn;
        public string? CancelReason { get; set; }
        public DateTime? CheckInTime { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual Membership? Membership { get; set; }
    }
}