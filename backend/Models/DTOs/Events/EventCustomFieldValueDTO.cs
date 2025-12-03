namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventCustomFieldValueDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventCustomFieldId { get; set; } = string.Empty;
        public string? EventRegistrationId { get; set; }
        public string? GuestListId { get; set; }
        public string FieldValue { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}

