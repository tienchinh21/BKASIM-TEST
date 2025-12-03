using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Fields
{
    public class FieldChild : BaseEntity
    {
        public string ChildName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FieldId { get; set; } = string.Empty;
    }
}
