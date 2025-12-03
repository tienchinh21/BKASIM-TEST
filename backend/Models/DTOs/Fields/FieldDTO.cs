namespace MiniAppGIBA.Models.DTOs.Fields
{
    public class FieldDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrderMiniApp { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<FieldChildDTO> Children { get; set; } = new List<FieldChildDTO>();
    }
}
