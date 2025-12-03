using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.EventRegistrations
{
    public class RegisterEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
        public List<RegisterEventCustomFieldValueRequest>? CustomFields { get; set; }
    }
    public class RegisterEventCustomFieldValueRequest
    {
        public string EventCustomFieldId { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty;
    }
}
