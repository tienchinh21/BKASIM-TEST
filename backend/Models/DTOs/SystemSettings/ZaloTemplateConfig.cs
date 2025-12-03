using System.Collections.Generic;

namespace MiniAppGIBA.Models.DTOs.SystemSettings
{
    public class ZaloTemplateConfig
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Message { get; set; }
        public List<string>? Parameters { get; set; }
        public Dictionary<string, string>? Mapping { get; set; }
    }
}
