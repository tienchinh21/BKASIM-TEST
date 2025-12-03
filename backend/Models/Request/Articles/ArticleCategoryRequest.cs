namespace MiniAppGIBA.Models.Request.Articles
{
    public class ArticleCategoryRequest
    {
        public required string Name { get; set; }
        public int DisplayOrder { get; set; } = 1;
    }
}
