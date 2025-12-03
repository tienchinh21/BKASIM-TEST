using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Models.Request.Rules
{
    public class CreateBehaviorRuleRequest
    {
        /// <summary>
        /// TEXT hoặc FILE
        /// </summary>
        public string ContentType { get; set; } = string.Empty; // "TEXT" | "FILE"

        /// <summary>
        /// APP hoặc GROUP
        /// </summary>
        public string Type { get; set; } = string.Empty; // "APP" | "GROUP"

        /// <summary>
        /// Bắt buộc nếu Type = GROUP
        /// </summary>
        public string? GroupId { get; set; }

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