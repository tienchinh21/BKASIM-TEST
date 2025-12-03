using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Request.Events;

namespace MiniAppGIBA.Services.Events
{
    public interface IEventCustomFieldService
    {
        Task<List<EventCustomFieldDTO>> GetEventCustomFieldsAsync(string eventId);
        Task<EventCustomFieldDTO> CreateEventCustomFieldAsync(CreateEventCustomFieldRequest request);
        Task<EventCustomFieldDTO> UpdateEventCustomFieldAsync(UpdateEventCustomFieldRequest request);
        Task<bool> DeleteEventCustomFieldAsync(string id);
        
        Task<List<EventCustomFieldValueDTO>> GetListValueCustomFieldsAsync(string? GuestListId = null, string? EventRegistrationId = null);
        Task<List<EventCustomFieldValueDTO>> CreateEventCustomFieldValuesAsync(string GuestListId, List<CreateEventCustomFieldValueRequest> requests);
    }
}