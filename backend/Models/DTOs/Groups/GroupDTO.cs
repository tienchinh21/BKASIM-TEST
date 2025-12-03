namespace MiniAppGIBA.Models.DTOs.Groups
{
    public class GroupDTO
    {
        public string Id { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Rule { get; set; }
        public bool IsActive { get; set; }
        public string? Logo { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsJoined { get; set; } = false; // Trạng thái user đã tham gia nhóm chưa
        public string? JoinStatus { get; set; } // "approved", "pending", "rejected", null
        public string? JoinStatusText { get; set; } // "Đã tham gia", "Chờ phê duyệt", "Bị từ chối", null
        public string? MainActivities { get; set; } // Các hoạt động chính
    }
}

