namespace MiniAppGIBA.Models.DTOs.Logs
{
    public class ReferralLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReferrerId { get; set; } = string.Empty;
        public string RefereeId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? ReferralCode { get; set; }
        public string? Source { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedDate { get; set; }

        // Thông tin bổ sung (join từ FK)
        public string? ReferrerName { get; set; }
        public string? RefereeName { get; set; }
        public string? GroupName { get; set; }
    }

    public class CreateReferralLogDto
    {
        public string ReferrerId { get; set; } = string.Empty;
        public string RefereeId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? ReferralCode { get; set; }
        public string? Source { get; set; }
        public string? Metadata { get; set; }
    }
}

