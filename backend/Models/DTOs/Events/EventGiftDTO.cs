namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventGiftDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string GiftName { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
