using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Groups
{
    public class UpdateJoinRequestRequest
    {
        public string? Reason { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
    }
}

