namespace MiniAppGIBA.Models.DTOs.Logs
{
    public class ProfileShareLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string SharerId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? SharedData { get; set; }
        public string? ShareMethod { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedDate { get; set; }

        // Thông tin bổ sung (join từ FK)
        public string? SharerName { get; set; }
        public string? ReceiverName { get; set; }
        public string? GroupName { get; set; }
    }

    public class CreateProfileShareLogDto
    {
        public string SharerId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? SharedData { get; set; }
        public string? ShareMethod { get; set; }
        public string? Metadata { get; set; }
    }
}

