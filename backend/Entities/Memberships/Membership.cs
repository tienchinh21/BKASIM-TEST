using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Entities.Memberships
{
    public class Membership : BaseEntity
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string UserZaloIdByOA { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? OldSlugs { get; set; }
        public string? ZaloAvatar { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FieldIds { get; set; }
        public string? Profile { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Position { get; set; }
        
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
        public string? CompanyLogo { get; set; } // Logo công ty
        public string? BusinessRegistrationNumber { get; set; } // Số giấy chứng nhận đăng ký kinh doanh
        public DateTime? BusinessRegistrationDate { get; set; } // Ngày cấp giấy chứng nhận
        public string? BusinessRegistrationPlace { get; set; } // Nơi cấp giấy chứng nhận

        //  Rating fields
        public decimal AverageRating { get; set; } = 0;    // Average rating (0-5)
        public int TotalRatings { get; set; } = 0;         // Total number of ratings

         public string? AppPosition { get; set; }
        public string? Term {get;set;} 
        public string Code {get;set;} = GenerateMembershipCode.GenerateCode();
        
        public string? RoleId { get; set; }


         public string? SortField {get;set;}
        // Soft delete flag
        public bool IsDelete { get; set; } = false;

        // Navigation properties
        public virtual ICollection<MembershipGroup> MembershipGroups { get; set; } = new List<MembershipGroup>();
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public virtual ICollection<EventGuest> EventGuests { get; set; } = new List<EventGuest>();
    }
}
