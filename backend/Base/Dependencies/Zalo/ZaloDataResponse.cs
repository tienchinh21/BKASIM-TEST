namespace MiniAppGIBA.Base.Dependencies.Zalo
{
    public class ZaloDataResponse
    {
        public Dictionary<string, dynamic>? data { get; set; }
        public int error { get; set; }
        public string? message { get; set; }
    }
}
