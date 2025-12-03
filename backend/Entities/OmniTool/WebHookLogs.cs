using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.OmniTool
{
    public class WebHookLogs : BaseEntity
    {
        public string? Status { get; set; }
        public string? Channel { get; set; }
        public string? Response { get; set; }
        public string? ErrorCode { get; set; }
        public string? IdOmniMess { get; set; }

        public int TelcoId { get; set; }
        public int MtCount { get; set; }
    }
}
