using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    public class CustomFieldTab : BaseEntity
    {
        public ECustomFieldEntityType EntityType { get; set; }

        public string EntityId { get; set; } = string.Empty;

        public string TabName { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        /// <summary>
        /// Soft delete - khi xóa tab, user vẫn thấy được data cũ
        /// </summary>
        public bool IsDelete { get; set; } = false;

        public virtual ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();
    }
}
