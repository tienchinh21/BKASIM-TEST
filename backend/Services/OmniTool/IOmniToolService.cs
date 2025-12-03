using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Entities.OmniTool;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Requests.OmniTools;

namespace MiniAppGIBA.Services.OmniTool
{
    public interface IOmniToolService
    {
        //void StopJob(List<string> jobIds);

        //Task<CampaignCSKH?> GetCampaignCSKHByIdAsync(string id);
        //Task<List<string>> GetTagsByCampaignId(string campaignId);
        //Task<(CampaignCSKH?, CampaignConfig?)> GetDetailCampaignById(string id);

        //Task<int> DeleteCampaignByIdAsync(string id);
        //Task<int> UpdateCampaignAsync(string id, CampaignRequest campaignRequest);
        //Task<int> CreateCampaignAsync(string accountId, CampaignRequest campaignRequest);
        //Task<PagedResult<CampaignCSKH>> GetPageCampaign(RequestQuery query);


        ////Task<PagedResult<WebHookLogs>> GetPageWebHookLogs(RequestQuery query, string? campaignId, short? status);
        //Task<PagedResult<CampaignPhoneCSKH>> GetPageCampaignPhoneLogs(RequestQuery query, string? campaignId, short? status);
        Task<PagedResult<EventTemplateLog>> GetEventTemplateLogs(RequestQuery query, string? status, string? type, DateTime? fromDate, DateTime? toDate);
        Task<byte[]> ExportEventTemplateLogsToExcel(string? status, string? type, DateTime? fromDate, DateTime? toDate, string? keyword = null);
    }
}
