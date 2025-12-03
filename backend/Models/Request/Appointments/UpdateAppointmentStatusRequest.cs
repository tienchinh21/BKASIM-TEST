using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Appointments
{
    public class UpdateAppointmentStatusRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public byte Status { get; set; }
        
        public string? CancelReason { get; set; }
    }
}

