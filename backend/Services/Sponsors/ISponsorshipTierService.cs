using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Sponsors;
using MiniAppGIBA.Models.Queries.Sponsors;
using MiniAppGIBA.Models.Request.Sponsors;

namespace MiniAppGIBA.Services.Sponsors
{
    public interface ISponsorshipTierService
    {
        Task<PagedResult<SponsorshipTierDTO>> GetSponsorshipTiersAsync(SponsorshipTierQueryParameters query);
        Task<SponsorshipTierDTO?> GetSponsorshipTierByIdAsync(string id);
        Task<SponsorshipTierDTO> CreateSponsorshipTierAsync(CreateSponsorshipTierRequest request);
        Task<SponsorshipTierDTO> UpdateSponsorshipTierAsync(string id, UpdateSponsorshipTierRequest request);
        Task<bool> DeleteSponsorshipTierAsync(string id);
        Task<List<SponsorshipTierDTO>> GetActiveSponsorshipTiersAsync();
    }
}
