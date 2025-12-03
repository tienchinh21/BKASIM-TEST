namespace MiniAppGIBA.Models.DTOs.Sponsors
{
    public class SponsorDTO
    {
        public string Id { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? Introduction { get; set; }
        public string? WebsiteURL { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
