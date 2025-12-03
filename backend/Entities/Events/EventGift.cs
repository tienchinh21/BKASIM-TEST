using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Events
{
    public class EventGift : BaseEntity
    {
        public string EventId { get; set; } = string.Empty;
        public string GiftName { get; set; } = string.Empty;
        public string? Images { get; set; }
        public int Quantity { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; } = null!;
    }
}