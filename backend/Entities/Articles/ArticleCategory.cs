using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Articles
{
    public class ArticleCategory : BaseEntity
    {
        public required string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
