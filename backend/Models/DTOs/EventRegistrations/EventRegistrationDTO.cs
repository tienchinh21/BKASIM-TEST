namespace MiniAppGIBA.Models.DTOs.EventRegistrations
{
    public class EventRegistrationDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CheckInCode { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public int IsCheckIn { get; set; }
        public string IsCheckInText { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class EventRegistrationResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? ZaloName { get; set; }
    }
}
