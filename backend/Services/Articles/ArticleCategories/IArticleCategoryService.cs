using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Models.Request.Articles;

namespace MiniAppGIBA.Services.Articles.ArticleCategories
{
    public interface IArticleCategoryService : IService<ArticleCategory>
    {
        Task<int> CreateAsync(ArticleCategoryRequest request);
        Task<int> UpdateAsync(string id, ArticleCategoryRequest request);
        Task<int> DeleteAsync(string id);
        Task<int> DeleteRange(List<string> categoryIds);
    }
}
