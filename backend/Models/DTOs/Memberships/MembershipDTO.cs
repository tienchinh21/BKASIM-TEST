namespace MiniAppGIBA.Models.DTOs.Memberships
{
    public class MembershipDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string UserZaloIdByOA { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? OldSlugs { get; set; }
        public string? ZaloAvatar { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? RoleId { get; set; }
        public bool IsDelete { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Personal information fields
        public string? FieldIds { get; set; }
        public string? Profile { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Position { get; set; }

        // Company information fields
        public string? CompanyFullName { get; set; }
        public string? CompanyBrandName { get; set; }
        public string? TaxCode { get; set; }
        public string? BusinessField { get; set; }
        public string? BusinessType { get; set; }
        public string? HeadquartersAddress { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyPhoneNumber { get; set; }
        public string? CompanyEmail { get; set; }
        public string? LegalRepresentative { get; set; }
        public string? LegalRepresentativePosition { get; set; }
        public string? CompanyLogo { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public DateTime? BusinessRegistrationDate { get; set; }
        public string? BusinessRegistrationPlace { get; set; }

        // Rating fields
        public decimal AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;

        // Other fields
        public string? AppPosition { get; set; }
        public string? Term { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? SortField { get; set; }
    }
}
