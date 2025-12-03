using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Events
{
    public class EventQueryParameters : BaseQueryParameters
    {
        public string? GroupId { get; set; }
        public new byte? Type { get; set; }
        public string? GroupType { get; set; }
        public new byte? Status { get; set; }
        public bool IsUser { get; set; } = true;
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}