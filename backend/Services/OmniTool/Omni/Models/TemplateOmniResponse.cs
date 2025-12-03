namespace MiniAppGIBA.Services.OmniTool.Omni.Models
{
    public class TemplateOmniResponse
    {
        public int Page { get; set; }
        public int TotalItem { get; set; }
        public List<TemplateResponse> ListTemp { get; set; } = new List<TemplateResponse>();
    }

    public class TemplateResponse
    {
        public string TemplateCode { get; set; } = string.Empty;
        public Dictionary<string, string>? SenderChannel { get; set; }
        public Dictionary<string, ParamFormatObj>? ParamsFormat { get; set; }
    }

    public class ParamFormatObj
    {
        public string? Regex { get; set; }
        public string? Type { get; set; }
    }
}
