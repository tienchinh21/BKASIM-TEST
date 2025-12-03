using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.HomePins
{
    /// <summary>
    /// DTO for HomePin entity with entity details
    /// </summary>
    public class HomePinDto
    {
        public string Id { get; set; } = string.Empty;
        public PinEntityType EntityType { get; set; }
        public string EntityTypeName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string PinnedBy { get; set; } = string.Empty;
        public string PinnedByName { get; set; } = string.Empty;
        public DateTime PinnedAt { get; set; }
        public string? Notes { get; set; }
        
        /// <summary>
        /// Entity details (Event, Meeting, or Showcase)
        /// </summary>
        public object? EntityDetails { get; set; }
    }
}
