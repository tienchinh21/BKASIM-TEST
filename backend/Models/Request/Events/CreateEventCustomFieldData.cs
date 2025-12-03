using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventCustomFieldData
    {
        public string FieldName { get; set; } = string.Empty;
        public EEventFieldType FieldType { get; set; }
        public string? FieldValue { get; set; }
        public bool IsRequired { get; set; }
    }
}

