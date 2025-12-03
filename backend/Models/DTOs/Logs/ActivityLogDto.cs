namespace MiniAppGIBA.Models.DTOs.Logs
{
    public class ActivityLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetEntity { get; set; }
        public string? TargetId { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedDate { get; set; }

        // Thông tin bổ sung (join từ FK)
        public string? AccountFullName { get; set; }
        public string? AccountEmail { get; set; }
    }

    public class CreateActivityLogDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetEntity { get; set; }
        public string? TargetId { get; set; }
        public string? Metadata { get; set; }
    }
}

