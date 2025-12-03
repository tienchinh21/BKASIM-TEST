namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventGiftData
    {
        public string GiftName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; } = true;
        public int? ImageIndex { get; set; } // Index of the image in GiftImages array
    }
}
