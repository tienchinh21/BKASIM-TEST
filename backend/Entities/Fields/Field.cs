using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Fields
{

    public class Field : BaseEntity
    {
        
        public string FieldName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DisplayOrderMiniApp { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<FieldChild> Children { get; set; } = new List<FieldChild>();
    }
}
