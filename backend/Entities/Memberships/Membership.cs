using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Entities.Memberships
{
    public class Membership : BaseEntity
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string UserZaloIdByOA { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? OldSlugs { get; set; }
        public string? ZaloAvatar { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        public string? RoleId { get; set; }

        // Soft delete flag
        public bool IsDelete { get; set; } = false;

        // Navigation properties
        public virtual ICollection<MembershipGroup> MembershipGroups { get; set; } = new List<MembershipGroup>();
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public virtual ICollection<EventGuest> EventGuests { get; set; } = new List<EventGuest>();
    }
}
