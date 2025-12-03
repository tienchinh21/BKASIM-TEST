using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Notifications
{
    public class EventTriggerLog : BaseEntity
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? Recipient { get; set; }

        public string? ResultCode { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }

        public string? Metadata { get; set; } // lưu lại thông tin metadata của event này
    }
}
