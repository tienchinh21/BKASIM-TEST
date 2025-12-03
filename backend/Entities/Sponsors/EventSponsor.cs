using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Sponsors
{
    public class EventSponsor : BaseEntity
    {
        public string EventId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorshipTierId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Events.Event Event { get; set; } = null!;
        public virtual Sponsor Sponsor { get; set; } = null!;
        public virtual SponsorshipTier SponsorshipTier { get; set; } = null!;
    }
}
