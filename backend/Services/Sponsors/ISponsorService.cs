using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Sponsors;
using MiniAppGIBA.Models.Queries.Sponsors;
using MiniAppGIBA.Models.Request.Sponsors;

namespace MiniAppGIBA.Services.Sponsors
{
    public interface ISponsorService
    {
        Task<PagedResult<SponsorDTO>> GetSponsorsAsync(SponsorQueryParameters query);
        Task<SponsorDTO?> GetSponsorByIdAsync(string id);
        Task<SponsorDTO> CreateSponsorAsync(CreateSponsorRequest request);
        Task<SponsorDTO> UpdateSponsorAsync(string id, UpdateSponsorRequest request);
        Task<bool> DeleteSponsorAsync(string id);
        Task<List<SponsorDTO>> GetActiveSponsorsAsync();
    }
}
