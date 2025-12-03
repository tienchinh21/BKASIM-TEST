using MiniAppGIBA.Models.Requests.OmniTools.Templates;

namespace MiniAppGIBA.Models.Requests.OmniTools
{
    public class OmniConfigRequest : EventTemplateV2Request
    {
        public required string PhoneNumber { get; set; }
        public required string TemplateCode { get; set; }
        public List<string> RoutingRule { get; set; } = new List<string>();
        public List<MappingParams> ParamsConfig { get; set; } = new List<MappingParams>();
    }
}
