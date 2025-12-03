using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class OmniTemplateConfig : BaseEntity
    {
        public required string PhoneNumber { get; set; }
        public required string RoutingRule { get; set; }
        public required string TemplateCode { get; set; }
        public required string TemplateMapping { get; set; }
    }
}
