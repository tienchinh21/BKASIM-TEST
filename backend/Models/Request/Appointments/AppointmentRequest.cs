namespace MiniAppGIBA.Models.Request.Appointments
{
    public class AppointmentRequest
    {
        public required string Name { get; set; }
        public required string AppointmentFrom { get; set; }
        public required string GroupId { get; set; }
        public required string AppointmentTo { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public required DateTime Time { get; set; }
        public string? CancelReason { get; set; }
    }
}