using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.Logs
{
    /// <summary>
    /// Bảng logs chia sẻ profile giữa các thành viên
    /// Tracking: User A chia sẻ profile cho User B
    /// </summary>
    public class ProfileShareLog : BaseEntity
    {
        /// <summary>
        /// FK -> Memberships.UserZaloId
        /// UserZaloId của người chia sẻ
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SharerId { get; set; } = string.Empty;

        /// <summary>
        /// FK -> Memberships.UserZaloId
        /// UserZaloId của người nhận
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReceiverId { get; set; } = string.Empty;

        /// <summary>
        /// FK -> Groups.Id
        /// Nhóm liên quan đến việc chia sẻ
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Dữ liệu được chia sẻ dạng JSON
        /// Ví dụ: { "fields": ["Name", "Phone", "Email"], "sharerInfo": {...}, "receiverInfo": {...} }
        /// </summary>
        public string? SharedData { get; set; }

        /// <summary>
        /// Phương thức chia sẻ: "QR_CODE", "LINK", "DIRECT"
        /// </summary>
        [MaxLength(20)]
        public string? ShareMethod { get; set; }

        /// <summary>
        /// Metadata dạng JSON
        /// Ví dụ: { "ipAddress": "...", "userAgent": "...", "shareTime": "...", "expiresAt": "..." }
        /// </summary>
        public string? Metadata { get; set; }
    }
}

