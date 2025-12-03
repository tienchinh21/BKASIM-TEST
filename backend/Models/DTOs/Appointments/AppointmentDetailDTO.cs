namespace MiniAppGIBA.Models.DTOs.Appointments
{
    public class AppointmentDetailDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        
        // Original IDs
        public string? AppointmentFromId { get; set; }
        public string? AppointmentToId { get; set; }
        public string? GroupId { get; set; }
        
        // Resolved Names
        public string? AppointmentFromName { get; set; }
        public string? AppointmentToName { get; set; }
        public string? GroupName { get; set; }
        
        // Avatar URLs
        public string? AppointmentFromAvatar { get; set; }
        public string? AppointmentToAvatar { get; set; }
        
        public string? CancelReason { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public DateTime? Time { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}

