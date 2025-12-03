using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventCustomFieldValueRequest
    {
        public string EventCustomFieldId { get; set; } = string.Empty;

        public string? FieldValue { get; set; }
    }
}

