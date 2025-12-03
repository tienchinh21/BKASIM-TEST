using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Requests.Common;
namespace MiniAppGIBA.Models.Queries.Meetings
{
    public class MeetingQueryParams : BaseQueryParameters
    {
        public string? GroupId { get; set; }
        public MeetingType? MeetingType { get; set; }
        public bool? IsPublic { get; set; }
        public DateTime? Time { get; set; }

        public new EMeetingStatus? Status { get; set; }
    }
}

