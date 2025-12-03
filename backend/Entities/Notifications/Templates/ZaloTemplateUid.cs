using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Notifications.Templates
{
    public class ZaloTemplateUid : BaseEntity
    {
        public string? Name { get; set; }
        public string? Message { get; set; }
        public string? ListParams { get; set; }
    }
}
