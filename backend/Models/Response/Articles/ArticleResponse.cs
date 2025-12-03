namespace MiniAppGIBA.Models.Response.Articles
{
    public class ArticleResponse
    {
        public string? Id { get; set; }
        public short Status { get; set; } = 0;
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Content { get; set; }
        public string? BannerImage { get; set; }
        public string? CategoryId { get; set; }
        public string? SummarizeContent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<string>? Images { get; set; } = new List<string>();

        public int OrderPriority { get; set; } = 1;

        /// <summary>
        /// Group category: "NBD", "Club", or "GIBA"
        /// </summary>
        public string? GroupCategory { get; set; }

        /// <summary>
        /// Comma-separated group IDs for multi-group assignment
        /// </summary>
        public string? GroupIds { get; set; }

        /// <summary>
        /// Resolved group names when displaying scope by explicit group IDs
        /// </summary>
        public List<string>? GroupNames { get; set; }
    }
}
