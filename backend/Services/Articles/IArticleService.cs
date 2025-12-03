using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Articles;
using MiniAppGIBA.Models.Request.Articles;

namespace MiniAppGIBA.Services.Articles
{
    public interface IArticleService : IService<Article>
    {
        Task<int> CreateAsync(ArticleRequest model);
        Task<int> UpdateAsync(string id, ArticleRequest model);
        Task<PagedResult<Article>> GetPage(ArticleQueryParams query);
    }
}