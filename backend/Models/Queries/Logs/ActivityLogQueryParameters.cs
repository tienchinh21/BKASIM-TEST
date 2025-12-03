using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Logs
{
    public class ActivityLogQueryParameters : BaseQueryParameters
    {
        public string? AccountId { get; set; }
        public string? ActionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

