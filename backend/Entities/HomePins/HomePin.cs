using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.HomePins
{
    /// <summary>
    /// Represents a pinned item on the home page
    /// </summary>
    public class HomePin : BaseEntity
    {
        /// <summary>
        /// Type of entity being pinned (Event, Meeting, or Showcase)
        /// </summary>
        [Required]
        public PinEntityType EntityType { get; set; }

        /// <summary>
        /// ID of the entity being pinned
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Display order on the home page (1 = first, 2 = second, etc.)
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// ID of the admin user who pinned this item
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string PinnedBy { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the item was pinned
        /// </summary>
        [Required]
        public DateTime PinnedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether this pin is currently active
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional notes about why this item was pinned
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
