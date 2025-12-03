using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Refs
{
    public class RefQueryParameters : BaseQueryParameters
    {
        public new string? Keyword { get; set; }
        public new byte? Status { get; set; }
        public new byte? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public byte? MinRating { get; set; }
        public byte? MaxRating { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "desc";
    }
}

