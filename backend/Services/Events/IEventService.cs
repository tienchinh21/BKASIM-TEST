using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;

namespace MiniAppGIBA.Services.Events
{
    public interface IEventService
    {
        Task<PagedResult<EventDTO>> GetEventsAsync(EventQueryParameters query, string? roleName = null, string? userId = null, string? userZaloId = null);
        Task<EventDetailDTO?> GetEventByIdAsync(string id);
        Task<EventDTO> CreateEventAsync(CreateEventRequest request);
        Task<EventDTO> UpdateEventAsync(string id, UpdateEventRequest request);
        Task<bool> DeleteEventAsync(string id);
        Task<bool> ToggleEventStatusAsync(string id);
        Task<List<EventDTO>> GetActiveEventsAsync(List<string>? allowedGroupIds = null);
        Task<List<EventDTO>> GetEventsByGroupAsync(string groupId);
        Task<object> GetEventCapacityInfoAsync(string eventId);
        Task<EventStatisticsDTO> GetEventStatisticsAsync(string eventId);
        Task<byte[]> ExportEventStatisticsAsync(string eventId);
        Task<List<EventDTO>> GuestGetEvent();
    }
}
