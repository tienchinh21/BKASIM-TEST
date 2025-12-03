using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.Logs
{
    /// <summary>
    /// Bảng logs giới thiệu thành viên
    /// Tracking: User A giới thiệu User B vào nhóm X
    /// </summary>
    public class ReferralLog : BaseEntity
    {
        /// <summary>
        /// FK -> Memberships.UserZaloId
        /// UserZaloId của người giới thiệu
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReferrerId { get; set; } = string.Empty;

        /// <summary>
        /// FK -> Memberships.UserZaloId
        /// UserZaloId của người được giới thiệu
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RefereeId { get; set; } = string.Empty;

        /// <summary>
        /// FK -> Groups.Id
        /// Nhóm mà người được giới thiệu tham gia
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Mã giới thiệu được sử dụng (nếu có)
        /// </summary>
        [MaxLength(20)]
        public string? ReferralCode { get; set; }

        /// <summary>
        /// Nguồn giới thiệu: "FACEBOOK", "ZALO", "DIRECT", "QR_CODE", "LINK"
        /// </summary>
        [MaxLength(50)]
        public string? Source { get; set; }

        /// <summary>
        /// Metadata dạng JSON
        /// Ví dụ: { "ipAddress": "...", "userAgent": "...", "referrerName": "...", "refereeName": "...", "registrationDate": "..." }
        /// </summary>
        public string? Metadata { get; set; }

        // ❌ KHÔNG dùng Navigation Properties
        // public virtual Membership? Referrer { get; set; }
        // public virtual Membership? Referee { get; set; }
        // public virtual Group? Group { get; set; }
    }
}

