namespace MiniAppGIBA.Models.Queries.Groups
{
    public class PendingApprovalQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? GroupId { get; set; }
        public string? Keyword { get; set; } // Search by member name, phone, company
        public bool? IsApproved { get; set; } // null = pending, true = approved, false = rejected
        public bool ShouldFilterByApprovalStatus { get; set; } = true; // false = get all statuses
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
