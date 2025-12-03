using MiniAppGIBA.Models.Requests.OmniTools.Templates;

namespace MiniAppGIBA.Models
{
    public class OmniTemplateConfig
    {
        public string? PhoneNumber { get; set; }
        public string? TemplateCode { get; set; }
        public List<string>? RoutingRules { get; set; }
        public List<MappingParams>? TemplateMapping { get; set; }
    }

    public class MappingParams
    {
        public string? ParamName { get; set; }
        public string? DefaultValue { get; set; }
        public string? MappingTableName { get; set; }
        public required string MappingColumnName { get; set; }
    }
}
