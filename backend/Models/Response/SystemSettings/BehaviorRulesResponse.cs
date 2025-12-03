namespace MiniAppGIBA.Models.Response.SystemSettings
{
    public class BehaviorRulesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BehaviorRulesData? Data { get; set; }
    }

    public class BehaviorRulesData
    {
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? FileSize { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
