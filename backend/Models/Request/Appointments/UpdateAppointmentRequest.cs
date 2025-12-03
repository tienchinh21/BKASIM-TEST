using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Appointments
{
    public class UpdateAppointmentRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public string? GroupId { get; set; }
        public string? AppointmentTo { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public DateTime? Time { get; set; }
    }
}

