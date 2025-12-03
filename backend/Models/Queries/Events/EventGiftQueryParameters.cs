using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Events
{
    public class EventGiftQueryParameters : BaseQueryParameters
    {
        public string? EventId { get; set; }
        public bool? IsActive { get; set; }
    }
}
