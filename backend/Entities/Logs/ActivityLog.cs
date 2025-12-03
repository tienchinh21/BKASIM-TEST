using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.Logs
{
    /// <summary>
    /// Bảng logs hoạt động của ADMIN
    /// Tracking: CREATE_ADMIN, UPDATE_ADMIN, DELETE_ADMIN, ASSIGN_GROUPS, LOGIN, LOGOUT, EXPORT_DATA, etc.
    /// </summary>
    public class ActivityLog : BaseEntity
    {
        /// <summary>
        /// FK -> AspNetUsers.Id (ApplicationUser)
        /// ID của ADMIN thực hiện hành động
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Loại hành động: CREATE_ADMIN, UPDATE_ADMIN, DELETE_ADMIN, ASSIGN_GROUPS, LOGIN, LOGOUT, EXPORT_DATA
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết hành động
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Entity bị tác động (ví dụ: "Admin", "Group", "Member")
        /// </summary>
        [MaxLength(100)]
        public string? TargetEntity { get; set; }

        /// <summary>
        /// ID của entity bị tác động
        /// </summary>
        [MaxLength(50)]
        public string? TargetId { get; set; }

        /// <summary>
        /// Metadata dạng JSON chứa thông tin chi tiết
        /// Ví dụ: { "ipAddress": "192.168.1.1", "userAgent": "...", "requestBody": {...}, "changes": {...} }
        /// </summary>
        public string? Metadata { get; set; }

        // ❌ KHÔNG dùng Navigation Properties
        // public virtual ApplicationUser? Account { get; set; }
    }
}

