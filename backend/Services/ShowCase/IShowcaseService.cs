using MiniAppGIBA.Entities.Showcase;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Showcase;
using MiniAppGIBA.Models.Request.Showcase;
namespace MiniAppGIBA.Services.ShowCase
{
    public interface IShowcaseService
    {
        Task<PagedResult<Showcase>> GetPage(ShowcaseQueryParams query, string roleId, string userId);
        Task<bool> CreateAsync(ShowcaseRequest request);
        Task<bool> UpdateAsync(string id, ShowcaseRequest request);
        Task<bool> DeleteAsync(string id);
        Task<Showcase?> GetByIdAsync(string id);
    }
}