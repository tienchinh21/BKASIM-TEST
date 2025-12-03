using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Groups
{
    public class Group : BaseEntity
    {
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Rule { get; set; }
        public bool IsActive { get; set; } = true;
        public string? BehaviorRulesUrl { get; set; }
        public string? Logo { get; set; }
        public string? MainActivities { get; set; }

        public virtual ICollection<MembershipGroup> MembershipGroups { get; set; } = new List<MembershipGroup>();
        public virtual ICollection<Events.Event> Events { get; set; } = new List<Events.Event>();
    }
}
