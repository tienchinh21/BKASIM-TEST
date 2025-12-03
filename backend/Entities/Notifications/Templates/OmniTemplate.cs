using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Notifications.Templates
{
    public class OmniTemplate : BaseEntity
    {
        public string? PhoneNumber { get; set; }
        public string? RoutingRule { get; set; }
        public string? TemplateCode { get; set; }
        public string? TemplateMapping { get; set; }
    }
}
