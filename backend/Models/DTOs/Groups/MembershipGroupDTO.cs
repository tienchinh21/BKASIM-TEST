namespace MiniAppGIBA.Models.DTOs.Groups
{
    public class MembershipGroupDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public string? ZaloAvatar { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? GroupPosition { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsApproved { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Status helpers
        public string StatusText => IsApproved switch
        {
            null => "Chờ xét duyệt",
            true => "Đã duyệt",
            false => "Từ chối"
        };

        public string StatusClass => IsApproved switch
        {
            null => "warning",
            true => "success",
            false => "danger"
        };
    }
}
