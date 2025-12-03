using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Models.Request.SystemSettings
{
    public class BehaviorRulesUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public bool IsSuperAdmin { get; set; }
        public string? UserId { get; set; }
        public List<string>? AdminGroups { get; set; }
        public string WebRootPath { get; set; } = string.Empty;
        public string? GroupId { get; set; }
    }
}
