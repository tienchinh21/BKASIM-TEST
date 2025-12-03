namespace MiniAppGIBA.Models.DTOs.Fields
{
    public class FieldChildDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FieldId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
