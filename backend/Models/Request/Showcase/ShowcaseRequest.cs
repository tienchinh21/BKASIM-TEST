namespace MiniAppGIBA.Models.Request.Showcase;

public class ShowcaseRequest
{
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MembershipId { get; set; } = string.Empty;
    public string MembershipName { get; set; } = string.Empty;
    public string? MemberShipAvatar { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public string? CreatedBy { get; set; } = string.Empty;
    public string? RoleName { get; set; } = string.Empty;
}