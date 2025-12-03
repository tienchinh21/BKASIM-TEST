using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.Response.Events
{
    /// <summary>
    /// Response cho thông tin sự kiện (MiniApp API)
    /// Bao gồm thông tin đăng ký và check-in của user
    /// </summary>
    public class EventResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Banner { get; set; }
        public List<string> Images { get; set; } = new();

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public EEventType Type { get; set; }
        public string? MeetingLink { get; set; }
        public string? GoogleMapURL { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; }
        public EEventStatus Status { get; set; }

        /// <summary>
        /// User đã đăng ký sự kiện này chưa
        /// </summary>
        public bool IsRegister { get; set; } = false;

        /// <summary>
        /// Trạng thái check-in của user
        /// </summary>
        public ECheckInStatus CheckInStatus { get; set; } = ECheckInStatus.NotCheckIn;

        /// <summary>
        /// Mã check-in của user (nếu đã đăng ký)
        /// </summary>
        public string? CheckInCode { get; set; }

        /// <summary>
        /// Thời gian check-in (nếu đã check-in)
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// ID đăng ký (nếu đã đăng ký)
        /// </summary>
        public string? RegistrationId { get; set; }
    }
}

