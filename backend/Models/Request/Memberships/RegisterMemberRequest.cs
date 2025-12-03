using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Models.Request.Memberships
{
    public class RegisterMemberRequest
    {
        [Required(ErrorMessage = "UserZaloId là bắt buộc")]
        public string UserZaloId { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserZaloName là bắt buộc")]
        public string UserZaloName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        // [Required(ErrorMessage = "Username là bắt buộc")]
        public string? Username { get; set; }

        // [Required(ErrorMessage = "Password là bắt buộc")]
        public string? Password { get; set; }
        public string? ZaloAvatar { get; set; }
        public string? Profile { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public string? Message { get; set; }
        public string? Reason { get; set; }
        public string? Object { get; set; }
        public string? Contribute { get; set; }
        public string? CareAbout { get; set; }
        public string? OtherContribute { get; set; }
        public string? Address { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? FieldIds { get; set; } // Comma-separated string for form-data

        // Company information fields
        public string? CompanyFullName { get; set; } // Tên công ty đầy đủ
        public string? CompanyBrandName { get; set; } // Tên thương hiệu
        public string? TaxCode { get; set; } // Mã số thuế
        public string? BusinessField { get; set; } // Ngành nghề kinh doanh
        public string? BusinessType { get; set; } // Loại hình doanh nghiệp
        public string? HeadquartersAddress { get; set; } // Địa chỉ trụ sở chính
        public string? CompanyWebsite { get; set; } // Website doanh nghiệp
        public string? CompanyPhoneNumber { get; set; } // SĐT doanh nghiệp
        public string? CompanyEmail { get; set; } // Email doanh nghiệp
        public string? LegalRepresentative { get; set; } // Người đại diện pháp lý
        public string? LegalRepresentativePosition { get; set; } // Chức vụ của người đại diện
        public IFormFile? CompanyLogoFile { get; set; } // Logo công ty file upload
        public string? BusinessRegistrationNumber { get; set; } // Số giấy chứng nhận đăng ký kinh doanh
        public DateTime? BusinessRegistrationDate { get; set; } // Ngày cấp giấy chứng nhận
        public string? BusinessRegistrationPlace { get; set; } // Nơi cấp giấy chứng nhận
    }
    public class RegisterMemberByAdminRequest
    {
        // [Required(ErrorMessage = "UserZaloId là bắt buộc")]
        public string? UserZaloId { get; set; }

        // [Required(ErrorMessage = "UserZaloName là bắt buộc")]
        public string? UserZaloName { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        // [Required(ErrorMessage = "Username là bắt buộc")]
        public string? Username { get; set; }

        // [Required(ErrorMessage = "Password là bắt buộc")]
        public string? Password { get; set; }
        public string? ZaloAvatar { get; set; }
        public string? Profile { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? FieldIds { get; set; } // Comma-separated string for form-data

        // Company information fields
        public string? CompanyFullName { get; set; } // Tên công ty đầy đủ
        public string? CompanyBrandName { get; set; } // Tên thương hiệu
        public string? TaxCode { get; set; } // Mã số thuế
        public string? BusinessField { get; set; } // Ngành nghề kinh doanh
        public string? BusinessType { get; set; } // Loại hình doanh nghiệp
        public string? HeadquartersAddress { get; set; } // Địa chỉ trụ sở chính
        public string? CompanyWebsite { get; set; } // Website doanh nghiệp
        public string? CompanyPhoneNumber { get; set; } // SĐT doanh nghiệp
        public string? CompanyEmail { get; set; } // Email doanh nghiệp
        public string? LegalRepresentative { get; set; } // Người đại diện pháp lý
        public string? LegalRepresentativePosition { get; set; } // Chức vụ của người đại diện
        public IFormFile? CompanyLogoFile { get; set; } // Logo công ty file upload
        public string? BusinessRegistrationNumber { get; set; } // Số giấy chứng nhận đăng ký kinh doanh
        public DateTime? BusinessRegistrationDate { get; set; } // Ngày cấp giấy chứng nhận
        public string? BusinessRegistrationPlace { get; set; } // Nơi cấp giấy chứng nhận
        public string? Message { get; set; }
        public string? Reason { get; set; }
        public string? Object { get; set; }
        public string? Contribute { get; set; }
        public string? CareAbout { get; set; }
        public string? OtherContribute { get; set; }
    }
}

