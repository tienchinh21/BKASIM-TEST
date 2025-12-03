using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Models.Queries.Articles
{
    public class ArticleQueryParams : RequestQuery, IRequestQuery
    {
        public short? Status { get; set; }
        public string? Type { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? CategoryId { get; set; }
        public string? ZaloUserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// User's group IDs for filtering (null = GIBA/all access, empty = NBD/Club, specific IDs = Group role)
        /// </summary>
        public List<string>? UserGroupIds { get; set; }

        /// <summary>
        /// Group type filter for NBD/Club roles ("NBD" or "Club")
        /// </summary>
        public string? GroupTypeFilter { get; set; }
    }
}
