using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    public class CustomField : BaseEntity
    {
        public string? CustomFieldTabId { get; set; }

        public ECustomFieldEntityType EntityType { get; set; }

        public string EntityId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;

        public EEventFieldType FieldType { get; set; }

        public string? FieldOptions { get; set; }

        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsDelete { get; set; } = false;

        public bool IsProfile { get; set; } = false;

        public virtual CustomFieldTab? CustomFieldTab { get; set; }
        public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}
