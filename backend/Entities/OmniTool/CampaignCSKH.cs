using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.OmniTool
{
    public class CampaignCSKH : BaseEntity
    {
        public short Status { get; set; }  // 1: Pending, 2: Running, 3: Done, 4: Cancelled
        public required string RoutingRule { get; set; }
        public required string TemplateCode { get; set; }
        public required string CampaignCode { get; set; }
        public required string CampaignName { get; set; }

        public byte UpdateCount { get; set; }
        public byte CampaignStatusID { get; set; }

        public string? Note { get; set; }
        public string? IdJob { get; set; }
        public string? AccountCmsId { get; set; }

        public int Total { get; set; }
        public int TotalSuccess { get; set; }

        public DateTime? UpdateTime { get; set; }
        public DateTime ScheduleTime { get; set; }
        public DateTime? OldScheduleTime { get; set; }

    }
}
