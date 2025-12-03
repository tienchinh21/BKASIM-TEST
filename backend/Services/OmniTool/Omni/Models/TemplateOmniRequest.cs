namespace MiniAppGIBA.Services.OmniTool.Omni.Models
{
    public class TemplateOmniRequest
    {
        public int Page { get; set; } = 0;
        public int ItemCount { get; set; } = 200;
        public string? TemplateCode { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
