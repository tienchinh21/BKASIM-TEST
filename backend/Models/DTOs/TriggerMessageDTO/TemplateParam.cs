namespace MiniAppGIBA.Models.DTOs.TriggerMessageDTO
{
    public class TemplateParam
    {
        public required string ParamName { get; set; }
        public required string TableMapping { get; set; }
        public required string FieldMapping { get; set; }
        public required string DefaultData { get; set; }

    }
}
