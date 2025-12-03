using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventCustomFieldDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty;
        public EEventFieldType? FieldType { get; set; }
        public string? FieldTypeText { get; set; }
        public bool IsRequired { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}

