using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Auth
{
    public class MiniAppLoginRequest
    {
        [Required(ErrorMessage = "UserZaloId là bắt buộc")]
        public string UserZaloId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}

