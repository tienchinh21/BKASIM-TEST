using MiniAppGIBA.Models.Requests.OmniTools.Templates;

namespace MiniAppGIBA.Models.Requests.OmniTools
{
    public class CampaignRequest : MappingTemplate
    {
        public required string Name { get; set; }
        public DateTime ScheduleTime { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
