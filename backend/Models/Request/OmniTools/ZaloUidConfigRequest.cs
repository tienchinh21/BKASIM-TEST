using MiniAppGIBA.Models.Requests.OmniTools.Templates;

namespace MiniAppGIBA.Models.Requests.OmniTools
{
    public class ZaloUidConfigRequest : EventTemplateV2Request
    {
        public required string ZaloTemplateUid { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public List<MappingParams> ParamsConfig { get; set; } = new List<MappingParams>();
    }
}
