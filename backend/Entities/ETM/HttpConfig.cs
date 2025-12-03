using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class HttpConfig : BaseEntity
    {
        public string? Method { get; set; }
        public string? Endpoint { get; set; }
        public string? HeadersJson { get; set; }
        public string? BodyJson { get; set; }
    }
}
