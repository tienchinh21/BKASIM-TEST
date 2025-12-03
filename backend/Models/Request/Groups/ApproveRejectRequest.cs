using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Groups
{
    public class ApproveRejectRequest
    {
        [Required(ErrorMessage = "Id yêu cầu không được để trống")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái phê duyệt không được để trống")]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Lý do từ chối (bắt buộc nếu IsApproved = false)
        /// </summary>
        public string? RejectReason { get; set; }

        /// <summary>
        /// ID gói cước (tùy chọn khi duyệt thành viên)
        /// </summary>
        public string? SubscriptionPlanId { get; set; }

        /// <summary>
        /// Số ngày thêm (tùy chọn)
        /// </summary>
        public int? AdditionalDays { get; set; }

        /// <summary>
        /// Ngày bắt đầu tùy chỉnh (tùy chọn)
        /// </summary>
        public DateTime? CustomStartDate { get; set; }
    }
}

