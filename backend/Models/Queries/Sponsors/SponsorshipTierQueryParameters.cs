using MiniAppGIBA.Models.Requests.Common;

namespace MiniAppGIBA.Models.Queries.Sponsors
{
    public class SponsorshipTierQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
    }
}
