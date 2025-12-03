using MiniAppGIBA.Models.Requests.OmniTools.Templates;

namespace MiniAppGIBA.Models.Requests.OmniTools
{
    public class EventTemplateRequest : MappingTemplate
    {
        public bool IsEnable { get; set; }
        public string? Type { get; set; }
        public string? EventName { get; set; }
        public string? Conditions { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
