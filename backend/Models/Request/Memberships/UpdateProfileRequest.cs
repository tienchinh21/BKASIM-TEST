namespace MiniAppGIBA.Models.Request.Memberships
{
    public class UpdateProfileRequest
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string? UserZaloIdByOA { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ZaloAvatar { get; set; }
        public string? RoleId { get; set; }
    }
}
