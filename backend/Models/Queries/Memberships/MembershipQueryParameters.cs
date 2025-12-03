using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Memberships
{
    public class MembershipQueryParameters : BaseQueryParameters
    {
        public string? Keyword { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortDirection { get; set; } = "desc";
    }
}
