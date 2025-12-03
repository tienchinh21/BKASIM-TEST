using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class EventLog : BaseEntity
    {
        public int TelcoId { get; set; }
        public string? Code { get; set; }
        public string? Status { get; set; }
        public string? IdOMniMess { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParamsContent { get; set; }
    }
}
