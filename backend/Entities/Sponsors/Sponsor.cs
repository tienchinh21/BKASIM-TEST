using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Sponsors
{
    public class Sponsor : BaseEntity
    {
        public string SponsorName { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? Introduction { get; set; }
        public string? WebsiteURL { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}
