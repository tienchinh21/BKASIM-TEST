using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Models.Queries.Showcase
{
    public class ShowcaseQueryParams : RequestQuery, IRequestQuery
    {
        public string? GroupName { get; set; }
        public string? GroupId { get; set; }
        public string? AuthorName { get; set; }
        public byte? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Type { get; set; }
        public bool? IsPublic { get; set; }
    }
}