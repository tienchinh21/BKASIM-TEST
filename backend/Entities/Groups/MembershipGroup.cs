using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.CustomFields;

namespace MiniAppGIBA.Entities.Groups
{
    public class MembershipGroup : BaseEntity
    {
        public string UserZaloId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? GroupPosition { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsApproved { get; set; } // null = chờ duyệt, true = duyệt, false = từ chối
        public string? RejectReason { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool HasCustomFieldsSubmitted { get; set; } = false; // Track if custom fields have been submitted

        // Navigation properties
        public virtual Memberships.Membership Membership { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
        public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}
