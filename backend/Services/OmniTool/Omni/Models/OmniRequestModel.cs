namespace MiniAppGIBA.Services.OmniTool.Omni.Models
{
    public class OmniRequestModel
    {
        public required string username { get; set; }
        public required string password { get; set; }
        public required string phonenumber { get; set; }
        public required List<string> routerule { get; set; } = new List<string>() { "1" };
        public required string templatecode { get; set; }
        public required Dictionary<string, string> list_param { get; set; }
    }
}
