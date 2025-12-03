using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Memberships
{
    public class RegisterMemberRequest
    {
        [Required(ErrorMessage = "UserZaloId là bắt buộc")]
        public string UserZaloId { get; set; } = string.Empty;

        public string UserZaloName { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? ZaloAvatar { get; set; }
    }

    public class RegisterMemberByAdminRequest
    {
        public string? UserZaloId { get; set; }

        public string? UserZaloName { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? ZaloAvatar { get; set; }

        public string? RoleId { get; set; }
    }
}
