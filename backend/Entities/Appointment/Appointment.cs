using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Appointment
{
    public class Appointment : BaseEntity
    {
        public string? Name { get; set; }
        public string? AppointmentFrom { get; set; }
        public string? GroupId { get; set; }
        public string? AppointmentTo { get; set; }
        public string? CancelReason { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public DateTime? Time { get; set; }
        public byte Status { get; set; } = 1;
    }
}