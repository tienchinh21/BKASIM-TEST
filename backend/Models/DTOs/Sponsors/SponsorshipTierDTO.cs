namespace MiniAppGIBA.Models.DTOs.Sponsors
{
    public class SponsorshipTierDTO
    {
        public string Id { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
