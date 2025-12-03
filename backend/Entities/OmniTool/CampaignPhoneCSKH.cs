using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.OmniTool
{
    public class CampaignPhoneCSKH : BaseEntity
    {
        public string? CampaignCSKHId { get; set; }
        public string? PhoneNumber { get; set; }

        public string? ErrorCode { get; set; }
        public string? IdOmniMess { get; set; }
        public string? ChannelCode { get; set; }
        public string? RoutingRule { get; set; }
        public string? TemplateCode { get; set; }
        public string? ParamContent { get; set; } // string JSON

        public short Status { get; set; }

        public short ReportTimes { get; set; }
        public short UpdateCount { get; set; }

        public int DelayTime { get; set; }
        public bool IsCharged { get; set; }

        public byte TelcoID { get; set; }
        public short ChannelID { get; set; }
        public short Duration { get; set; }
        public short MtCount { get; set; }

        public int ExtraDuration { get; set; }

        public string? AccountId { get; set; }

        public DateTime? ProcessTime { get; set; }
        public DateTime? IndexCrDateTime { get; set; }
        public DateTime? IndexProcessTime { get; set; }
        public DateTime? ReportedDateTime { get; set; }
    }
}
