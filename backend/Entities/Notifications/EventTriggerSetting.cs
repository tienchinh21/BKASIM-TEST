using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Notifications
{
    public class EventTriggerSetting : BaseEntity
    {
        public int Type { get; set; } // 1.Omni ; 2.UID ; 3.Email
        public string? EventName { get; set; }
        public string? Conditions { get; set; }
        public string? ReferenceId { get; set; }
        public string? ProcessingStep { get; set; }
        // danh sách người nhận
        public required string Recipients { get; set; } // danh sách người yêu cầu

        public bool IsActive { get; set; }
    }
}
