using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Sponsors
{
    public class SponsorQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
    }
}
