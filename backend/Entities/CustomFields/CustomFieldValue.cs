using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    public class CustomFieldValue : BaseEntity
    {
        public string CustomFieldId { get; set; } = string.Empty;

        public ECustomFieldEntityType EntityType { get; set; }
        public string EntityId { get; set; } = string.Empty;

        public string FieldName { get; set; } = string.Empty;

        public string FieldValue { get; set; } = string.Empty;
        
        public virtual CustomField CustomField { get; set; } = null!;
    }
}
