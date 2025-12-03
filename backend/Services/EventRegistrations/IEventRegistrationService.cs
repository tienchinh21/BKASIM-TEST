using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.EventRegistrations;
using MiniAppGIBA.Models.Request.EventRegistrations;

namespace MiniAppGIBA.Services.EventRegistrations
{
    public interface IEventRegistrationService
    {
        Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(string eventId, int page = 1, int pageSize = 10, string? keyword = null);
        Task<EventRegistrationResponseDTO> CheckInAsync(string checkInCode, string eventId);
        Task<List<EventRegistrationResponseDTO>> CheckInMultipleAsync(List<string> checkInCodes, string eventId);
        Task<bool> CancelByCodeAsync(string checkInCode, string eventId);
        Task<byte[]> ExportParticipantsAsync(string eventId);

        // Mini app APIs
        Task<EventRegistrationDTO> RegisterEventAsync(string eventId, string userZaloId, RegisterEventRequest request);
        Task<List<EventRegistrationDTO>> GetUserEventRegistrationsAsync(string userZaloId);
        Task<PagedResult<object>> GetAllUserRegistrationsAsync(string userZaloId, int page = 1, int pageSize = 10, int? type = null, string? keyword = null, byte? status = null);
        Task<bool> CancelEventRegistrationAsync(string eventId, string userZaloId);
        Task<object> GetEventDetailForUserAsync(string eventId, string userZaloId);
    }
}
