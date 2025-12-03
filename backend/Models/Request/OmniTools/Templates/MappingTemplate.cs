namespace MiniAppGIBA.Models.Requests.OmniTools.Templates
{
    public class MappingTemplate
    {
        public required string TemplateCode { get; set; }
        public List<string> RoutingRule { get; set; } = new List<string>();
        public List<MappingParams> ParamsConfig { get; set; } = new List<MappingParams>();
    }
}
