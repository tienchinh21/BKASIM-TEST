namespace MiniAppGIBA.Models.Requests.OmniTools
{
    public class EventTemplateV2Request
    {
        public bool IsEnable { get; set; }

        public string? Type { get; set; }
        public string? EventName { get; set; }
        public string? Conditions { get; set; }
    }
}
