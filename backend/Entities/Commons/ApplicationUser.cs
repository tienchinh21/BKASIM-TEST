using Microsoft.AspNetCore.Identity;

namespace MiniAppGIBA.Entities.Commons
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Trạng thái active của user (cho ADMIN)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Lần đăng nhập cuối cùng (cho ADMIN)
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        // ❌ KHÔNG thêm Navigation Properties để tránh ảnh hưởng logic cũ
        // public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
        // public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}
