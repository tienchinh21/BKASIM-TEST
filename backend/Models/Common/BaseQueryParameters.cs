namespace MiniAppGIBA.Models.Requests.Common
{
    public class BaseQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Sort { get; set; }
        public string? Keyword { get; set; }
        public string? Type { get; set; }
        public bool? Status { get; set; }
        public int Draw { get; set; } // For DataTables
    }
}
