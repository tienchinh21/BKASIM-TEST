using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Logs
{
    public class ProfileShareLogQueryParameters : BaseQueryParameters
    {
        public List<string>? GroupIds { get; set; }
        public string? SharerId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

