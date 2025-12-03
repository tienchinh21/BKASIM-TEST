namespace MiniAppGIBA.Models.DTOs.Admins
{
    public class GroupPermissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Thông tin bổ sung (join từ FK)
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public string? GroupName { get; set; }
    }

    public class CreateGroupPermissionDto
    {
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
    }

    public class UpdateGroupPermissionDto
    {
        public bool IsActive { get; set; }
    }
}

