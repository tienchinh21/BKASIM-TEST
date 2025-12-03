namespace MiniAppGIBA.Models.Response.SystemSettings
{
    public class BehaviorRulesFileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? FileSize { get; set; }
    }
}
