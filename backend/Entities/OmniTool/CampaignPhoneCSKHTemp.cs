using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.OmniTool
{
    public class CampaignPhoneCSKHTemp : BaseEntity
    {
        public string? CampaignCSHKId { get; set; }

        public short Status { get; set; }
        public byte TelcoID { get; set; }

        public string? RequestID { get; set; }
        public required string RoutingRule { get; set; }
        public required string TemplateCode { get; set; }
        public required string PhoneNumber { get; set; }
        public required string ParamContent { get; set; }

        public string? AccountId { get; set; } // Account CMS nào khởi tạo cái campaing này
    }
}
