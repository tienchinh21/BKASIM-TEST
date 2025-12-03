using System.Drawing;
using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Showcase
{
    public class Showcase : BaseEntity
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MembershipId { get; set; } = string.Empty;
        public string MembershipName { get; set; } = string.Empty;
        public string MemberShipAvatar { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public bool IsPublic { get; set; } = false;
        public byte Status { get; set; } = 1; // 1: Đã lên lịch, 2: Đang diễn ra, 3: Đã hoàn thành, 4: Đã hủy
        public string RoleId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
    }
}