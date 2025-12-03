using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;


namespace MiniAppGIBA.Services.OmniTool.TemplateUids
{
    public interface ITemplateUidService : IService<ZaloTemplateUid>
    {
        Task<int> UpdateAsync(string id, ZaloTemplateUid dto);
    }
}
