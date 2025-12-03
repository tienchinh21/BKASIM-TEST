namespace MiniAppGIBA.Models.Request.Memberships
{
    public class CreateMembershipRequest
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ZaloAvatar { get; set; }
        public string? RoleId { get; set; }
    }
}
