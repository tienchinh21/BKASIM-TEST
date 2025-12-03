namespace MiniAppGIBA.Models.DTOs.Memberships
{
    public class MembershipDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string UserZaloIdByOA { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? OldSlugs { get; set; }
        public string? ZaloAvatar { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? RoleId { get; set; }
        public bool IsDelete { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
