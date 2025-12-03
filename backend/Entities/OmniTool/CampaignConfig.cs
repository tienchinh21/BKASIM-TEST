using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.OmniTool
{
    public class CampaignConfig : BaseEntity
    {
        public string? CampaignId { get; set; }

        public string? TemplateCode { get; set; }
        public string? VariableContent { get; set; }
        public string? TemplateVariable { get; set; }

        public string? TagContent { get; set; }
        public string? TagAttribute { get; set; }
    }
}
