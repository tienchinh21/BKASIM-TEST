using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class CampaignItem : BaseEntity
    {
        public bool IsUsed { get; set; }
        public string? Key { get; set; }
        public string? VoucherCode { get; set; }
        public string? CampaignKey { get; set; }
    }
}
