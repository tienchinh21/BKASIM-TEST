namespace MiniAppGIBA.Models.Request.Articles
{
    public class ArticleRequest
    {
        public string? Author { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string CategoryId { get; set; }
        public string? Role { get; set; }
        public string? CreatedBy { get; set; }
        public IFormFile? BannerImage { get; set; }
        public List<IFormFile>? Images { get; set; }
        public short Status { get; set; } = 0;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public List<string> RemovedOldImages { get; set; } = new();
        public string RemovedOldBanner { get; set; } = string.Empty;

        public int OrderPriority { get; set; }

        /// <summary>
        /// Group category: "NBD", "Club", or "GIBA"
        /// </summary>
        public string? GroupCategory { get; set; }

        /// <summary>
        /// Comma-separated group IDs for multi-group assignment
        /// Example: "1,5,7"
        /// </summary>
        public string? GroupIds { get; set; }
    }
}
