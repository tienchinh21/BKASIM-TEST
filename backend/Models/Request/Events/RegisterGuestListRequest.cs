using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Events
{
    public class RegisterGuestListRequest
    {
        [Required(ErrorMessage = "Ghi chú/lý do là bắt buộc")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng khách mời là bắt buộc")]
        [Range(1, 50, ErrorMessage = "Số lượng khách mời phải từ 1 đến 50")]
        public int GuestNumber { get; set; }

        [Required(ErrorMessage = "Danh sách khách mời không được để trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 khách mời")]
        [MaxLength(50, ErrorMessage = "Không thể đăng ký quá 50 khách mời cùng lúc")]
        public List<GuestInfo> GuestList { get; set; } = new List<GuestInfo>();
    }

    public class GuestInfo
    {
        [Required(ErrorMessage = "Tên khách là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên khách không được vượt quá 100 ký tự")]
        public string GuestName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? GuestPhone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? GuestEmail { get; set; }
    }
}

