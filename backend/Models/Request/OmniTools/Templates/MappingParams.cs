namespace MiniAppGIBA.Models.Requests.OmniTools.Templates
{
    public class MappingParams
    {
        public string? ParamName { get; set; }
        public string? DefaultValue { get; set; }
        public string? MappingTableName { get; set; }
        public required string MappingColumnName { get; set; }
    }
}
