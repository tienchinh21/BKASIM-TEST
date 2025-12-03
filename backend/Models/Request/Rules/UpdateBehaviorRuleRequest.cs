using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Models.Request.Rules
{
    public class UpdateBehaviorRuleRequest
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// APP hoặc GROUP (để xác nhận)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// TEXT hoặc FILE
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung text khi ContentType = TEXT
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// File upload khi ContentType = FILE
        /// </summary>
        public IFormFile? File { get; set; }

        public string? Title { get; set; }
        public int? SortOrder { get; set; }
    }
}