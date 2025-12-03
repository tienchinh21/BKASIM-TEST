using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Refs
{
    public class CreateRefRequest
    {
        // [Required(ErrorMessage = "Người nhận là bắt buộc")]
        public string? RefTo { get; set; }

        // [Required(ErrorMessage = "Nội dung là bắt buộc")]
        // [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string? Content { get; set; }

        // [Required(ErrorMessage = "Nhóm là bắt buộc")]
        public string? GroupId { get; set; }

        /// <summary>
        /// GroupId của người nhận (RefTo) - chỉ dùng cho Type 0. Nếu không có thì dùng GroupId
        /// </summary>
        public string? RefToGroupId { get; set; }

        /// <summary>
        /// GroupId của người được share (ReferredMemberId) - dùng cho cả Type 0 và Type 1. Nếu không có thì dùng GroupId
        /// </summary>
        public string? ReferredMemberGroupId { get; set; }

        /// <summary>
        /// Type: 0 - gửi cho thành viên; 1 - gửi cho bên ngoài
        /// </summary>
        [Required(ErrorMessage = "Loại ref là bắt buộc")]
        [Range(0, 1, ErrorMessage = "Type phải là 0 (thành viên) hoặc 1 (bên ngoài)")]
        public byte Type { get; set; } = 0;

        /// <summary>
        /// ShareType: "own" - profile bản thân, "member" - profile thành viên trong nhóm, "external" - soạn text người ngoài
        /// </summary>
        public string? ShareType { get; set; } // "own", "member", "external"

        /// <summary>
        /// UserZaloId của thành viên được share (nếu ShareType = "member")
        /// </summary>
        public string? ReferredMemberId { get; set; }

        /// <summary>
        /// Thông tin người ngoài được share (nếu ShareType = "external")
        /// </summary>
        public string? ReferralName { get; set; }
        public string? ReferralPhone { get; set; }
        public string? ReferralEmail { get; set; }
        public string? ReferralAddress { get; set; }

        /// <summary>
        /// Thông tin người nhận bên ngoài (chỉ dùng cho Type 1)
        /// </summary>
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
    }
}
