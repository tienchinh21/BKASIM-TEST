namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventStatisticsDTO
    {
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Address { get; set; }
        public string? MeetingLink { get; set; }
        
        public int TotalRegistrations { get; set; }
        public int TotalCheckedIn { get; set; }
        public int TotalNotCheckedIn { get; set; }
        public int TotalCancelled { get; set; }
        public double AttendanceRate { get; set; }
        
        public List<EventParticipantDTO> Participants { get; set; } = new List<EventParticipantDTO>();
    }

    public class EventParticipantDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CheckInCode { get; set; }
        public DateTime RegisteredDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string ParticipantType { get; set; } = "Đăng ký"; // "Đăng ký" hoặc "Khách mời"
        public Dictionary<string, string> CustomFieldValues { get; set; } = new Dictionary<string, string>();
    }
}

