using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Models.Requests.OmniTools;

namespace MiniAppGIBA.Services.OmniTool.EventTemplates
{
    public interface IEventTemplateService : IService<EventTemplate>
    {
        Task<int> CreateAsync(EventTemplateRequest campaignRequest);
        Task<int> UpdateAsync(string id, EventTemplateRequest campaignRequest);

        Task<ZaloTemplateConfig> GetZaloUidConfigById(string id);
        Task<int> CreateAsync(ZaloUidConfigRequest zaloUidConfigRequest);
        Task<int> UpdateAsync(string id, ZaloUidConfigRequest zaloUidConfigRequest);
    }
}
