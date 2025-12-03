using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;

namespace MiniAppGIBA.Services.Events
{
    public interface IEventGiftService
    {
        Task<PagedResult<EventGiftDTO>> GetEventGiftsAsync(EventGiftQueryParameters query);
        Task<EventGiftDTO?> GetEventGiftByIdAsync(string id);
        Task<EventGiftDTO> CreateEventGiftAsync(CreateEventGiftRequest request);
        Task<EventGiftDTO> UpdateEventGiftAsync(string id, UpdateEventGiftRequest request);
        Task<bool> DeleteEventGiftAsync(string id);
        Task<bool> ToggleEventGiftStatusAsync(string id);
        Task<List<EventGiftDTO>> GetGiftsByEventAsync(string eventId);
    }
}
