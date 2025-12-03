namespace MiniAppGIBA.Models.DTOs.Events
{
    public class GuestListDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventGuestId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? CheckInCode { get; set; }
        public bool? CheckInStatus { get; set; }
    }
}

