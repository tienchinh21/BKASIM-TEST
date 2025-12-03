using MiniAppGIBA.Entities.Meetings;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Meetings;
using MiniAppGIBA.Models.Queries.Meetings;
using MiniAppGIBA.Models.Request.Meetings;

namespace MiniAppGIBA.Services.Meetings
{
    public interface IMeetingService
    {
        Task<PagedResult<Meeting>> GetPage(MeetingQueryParams query, string roleId, string userId);
        Task<bool> CreateAsync(MeetingRequest request);
        Task<bool> UpdateAsync(string id, MeetingRequest request);
        Task<bool> DeleteAsync(string id);
        Task<Meeting?> GetByIdAsync(string id);
    }
}

