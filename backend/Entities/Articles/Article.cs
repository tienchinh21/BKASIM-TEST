using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Articles
{
    public class Article : BaseEntity
    {
        public required string BannerImage { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Content { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public short Status { get; set; } = 0;
        public string? Images { get; set; }
        public string? Role { get; set; }
        public string? CategoryId { get; set; }

        /// <summary>
        /// Group category: "NBD", "Club", or "GIBA"
        /// Determines which category this article belongs to
        /// </summary>
        public string? GroupCategory { get; set; }

        /// <summary>
        /// Comma-separated group IDs for multi-group assignment
        /// Example: "1,5,7" means article is visible to groups 1, 5, and 7
        /// If null or empty: article is visible to ALL groups in the GroupCategory
        /// </summary>
        public string? GroupIds { get; set; }

        public int OrderPriority { get; set; } = 1;
    }
}
