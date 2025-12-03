using MiniAppGIBA.Base.Interface;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Common
{
    public class RequestQuery : IRequestQuery
    {
        private int _pageSize = 10;
        private int _page = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page
        {
            get => _page;
            set => _page = value > 0 ? value : 1;
        }

        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters")]
        public string? Keyword { get; set; } = string.Empty;

        public int Skip => (Page - 1) * PageSize;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value is > 0 and <= 100 ? value : 10;
        }

        // Additional properties for sorting
        public string? OrderBy { get; set; }
        public string? OrderType { get; set; } = "asc";

        public bool IsDescending => OrderType?.ToLowerInvariant() == "desc";
    }
}
