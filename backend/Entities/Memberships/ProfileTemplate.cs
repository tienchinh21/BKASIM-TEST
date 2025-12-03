using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.Memberships
{
    /// <summary>
    /// Mẫu profile tùy chỉnh cho mỗi thành viên
    /// Lưu cấu hình ẩn/hiện trường, ảnh bìa, màu chủ đề, v.v.
    /// </summary>
    public class ProfileTemplate : BaseEntity
    {
        /// <summary>
        /// FK -> Memberships.UserZaloId
        /// Liên kết với người dùng
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string UserZaloId { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách trường được phép hiển thị (JSON array)
        /// Ví dụ: ["fullname", "email", "company", "position", "zaloAvatar", "profile"]
        /// </summary>
        public string? VisibleFields { get; set; }

        /// <summary>
        /// Danh sách trường bị ẩn (JSON array)
        /// Ví dụ: ["phoneNumber", "address", "dayOfBirth"]
        /// </summary>
        public string? HiddenFields { get; set; }

        /// <summary>
        /// Mô tả profile tùy chỉnh
        /// </summary>
        public string? CustomDescription { get; set; }

        /// <summary>
        /// Ảnh bìa profile (đường dẫn file)
        /// Ví dụ: /uploads/profile/cover-images/user-123.jpg
        /// </summary>
        public string? CoverImage { get; set; }

        /// <summary>
        /// Màu chủ đề (hex color)
        /// Ví dụ: #0066cc
        /// </summary>
        public string? ThemeColor { get; set; }

        /// <summary>
        /// Trạng thái công khai
        /// true = ai cũng có thể xem profile
        /// false = chỉ người được chia sẻ mới xem
        /// </summary>
        public bool IsPublic { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ProfileCustomField> CustomFields { get; set; } = new List<ProfileCustomField>();
    }
}
