using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Fields
{
    public class FieldQueryParameters : BaseQueryParameters
    {
        public string? FieldName { get; set; }
        public bool? IsActive { get; set; }

        // Additional properties for sorting
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
