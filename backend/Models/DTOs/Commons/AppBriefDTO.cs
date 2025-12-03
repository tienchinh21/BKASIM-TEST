namespace MiniAppGIBA.Models.DTOs.Commons
{
    public class AppBriefDTO
    {
        public string Content { get; set; } = string.Empty;
        public bool IsPdf { get; set; }
        public string? PdfUrl { get; set; }
        public string? PdfFileName { get; set; }
        public string? FullUrl { get; set; }
    }
}

