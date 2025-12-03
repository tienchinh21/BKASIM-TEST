using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAppGIBA.Entities.Memberships
{
    /// <summary>
    /// Trường thông tin tùy chỉnh trong profile template
    /// Cho phép người dùng thêm các trường bổ sung ngoài các trường chuẩn
    /// </summary>
    public class ProfileCustomField : BaseEntity
    {
        /// <summary>
        /// FK -> ProfileTemplate.Id
        /// Liên kết với template profile
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string ProfileTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// Tên trường tùy chỉnh
        /// Ví dụ: "Sở thích", "Kỹ năng", "Chứng chỉ"
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Giá trị của trường
        /// Ví dụ: "Lập trình, Thiết kế"
        /// </summary>
        public string? FieldValue { get; set; }

        /// <summary>
        /// Loại trường
        /// Giá trị: "text", "textarea", "image", "link", "number", "date"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string FieldType { get; set; } = "text";

        /// <summary>
        /// Thứ tự hiển thị (từ nhỏ đến lớn)
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trường có được hiển thị không
        /// </summary>
        public bool IsVisible { get; set; } = true;

        // Navigation properties
        [ForeignKey("ProfileTemplateId")]
        public virtual ProfileTemplate? ProfileTemplate { get; set; }
    }
}
