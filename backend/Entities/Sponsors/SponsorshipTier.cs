using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Sponsors
{
    public class SponsorshipTier : BaseEntity
    {
        public string TierName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Image { get; set; }

        // Navigation properties
        public virtual ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}
