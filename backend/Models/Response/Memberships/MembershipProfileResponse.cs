namespace MiniAppGIBA.Models.Response.Memberships
{
    public class MembershipProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string UserZaloName { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ZaloAvatar { get; set; }
        public string? Profile { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public List<string> FieldNames { get; set; } = new();
        public int TotalGroups { get; set; }
        public int TotalEvents { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

