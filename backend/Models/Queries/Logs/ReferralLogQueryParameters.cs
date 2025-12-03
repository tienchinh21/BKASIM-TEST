using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Logs
{
    public class ReferralLogQueryParameters : BaseQueryParameters
    {
        public List<string>? GroupIds { get; set; }
        public string? ReferrerId { get; set; }
        public string? RefereeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

