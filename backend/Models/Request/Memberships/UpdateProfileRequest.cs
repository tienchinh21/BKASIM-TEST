namespace MiniAppGIBA.Models.Request.Memberships
{
    public class UpdateProfileRequest
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string? UserZaloIdByOA { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ZaloAvatar { get; set; }
        public string? RoleId { get; set; }

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

        // Other fields
        public string? AppPosition { get; set; }
        public string? Term { get; set; }
        public string? SortField { get; set; }
    }
}
