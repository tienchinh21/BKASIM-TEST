namespace MiniAppGIBA.Models.Request.Memberships
{
    public class UpdateMembershipRequest
    {
        public string? UserZaloName { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string? ZaloAvatar { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleId { get; set; }
    }
}
