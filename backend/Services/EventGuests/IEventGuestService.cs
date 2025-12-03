using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Models.Response.Events;

namespace MiniAppGIBA.Services.EventGuests
{
    public interface IEventGuestService
    {
        Task<EventGuestDTO> RegisterGuestListAsync(string eventId, string userZaloId, RegisterGuestListRequest request);
        Task<EventGuestDTO> UpdateGuestListAsync(string eventGuestId, string userZaloId, RegisterGuestListRequest request);
        Task<EventGuestDTO> ApproveGuestListAsync(string eventGuestId, ApproveGuestListRequest request);
        Task<EventGuestDTO> CancelGuestListAsync(string eventGuestId, string cancelReason);
        Task<EventGuestDTO?> GetEventGuestByIdAsync(string eventGuestId);
        Task<List<EventGuestDTO>> GetMyGuestListsAsync(string userZaloId);
        Task<PagedResult<EventGuestDTO>> GetMyGuestListsAsync(string userZaloId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null);
        Task<List<EventGuestDTO>> GetEventGuestListsAsync(string eventId);
        Task<PagedResult<EventGuestDTO>> GetEventGuestListsAsync(string eventId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null);
        Task<List<EventGuestDTO>> GetAllEventGuestsAsync();
        Task<List<EventGuestDTO>> GetEventGuestListsWithRegistrationsAsync(string eventId, List<string>? allowedGroupIds = null);
        Task<List<EventGuestDTO>> GetAllEventGuestsWithRegistrationsAsync(List<string>? allowedGroupIds = null);
        Task<EventGuestDTO> ApproveGuestListItemAsync(string guestListId, ApproveGuestListRequest request);
        Task<GuestListDTO> CancelGuestListItemAsync(string guestListId, string cancelReason);
        Task<List<EventGuestByPhoneResponse>> GetEventByPhoneAsync(string phone);
	        Task<List<EventGuestByPhoneResponse>> GetConfirmedEventsByPhoneAsync(string phone);
    }
}

