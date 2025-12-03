using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Groups
{
    public class GroupQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
        public string? JoinStatus { get; set; } // "approved", "pending", "rejected", "available"
        public string? GroupType { get; set; }
        public string? GroupId { get; set; }
    }
}

