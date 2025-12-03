using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class EventTemplate : BaseEntity
    {
        public required string Type { get; set; }
        public required string EventName { get; set; }
        public bool IsEnable { get; set; }
        public string? Conditions { get; set; }
        public string? ReferenceId { get; set; }

        // lược bỏ dần nha
        public required string PhoneNumber { get; set; }
        public required string RoutingRule { get; set; }
        public required string TemplateCode { get; set; }
        public required string TemplateMapping { get; set; }
    }
}
