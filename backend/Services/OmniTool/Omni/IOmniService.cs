using MiniAppGIBA.Models.DTOs.SystemSettings;
using MiniAppGIBA.Services.OmniTool.Omni.Models;

namespace MiniAppGIBA.Services.OmniTool.Omni
{
    public interface IOmniService
    {
        Task<TemplateOmniResponse> GetAllOwnedTemplate();
        Task<TemplateOmniResponse> GetAllOwnedTemplate(OmniAccountDTO omniAccountDTO);

        Task<SendOmniMessgeResponse> SendOmniMessageAsync(string templateCode, string phoneNumber, string routeRule, Dictionary<string, string> listParams);
        Task<SendOmniMessgeResponse> SendOmniMessageAsync(OmniAccountDTO omniAccountDTO, string templateCode, string phoneNumber, string routeRule, Dictionary<string, string> listParams, List<string> datasourceETMId);
    }
}
