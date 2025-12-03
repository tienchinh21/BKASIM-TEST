using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Memberships
{
    public class MembershipApprovalRequest
    {
        [Required(ErrorMessage = "ID thành viên là bắt buộc")]
        public string MembershipId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái phê duyệt là bắt buộc")]
        public byte ApprovalStatus { get; set; }  // 1 = Approved, 2 = Rejected

        public string? ApprovalReason { get; set; }  // Reason for rejection (required if rejected)
    }
}

