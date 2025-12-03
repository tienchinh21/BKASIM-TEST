using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    /// <summary>
    /// Represents a tab (topic) for organizing custom fields within an entity
    /// </summary>
    public class CustomFieldTab : BaseEntity
    {
        /// <summary>
        /// The type of entity this tab belongs to (e.g., GroupMembership, EventRegistration)
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the specific entity instance (e.g., GroupId for GroupMembership)
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the tab (e.g., "Personal Information", "Professional Details")
        /// </summary>
        public string TabName { get; set; } = string.Empty;

        /// <summary>
        /// The display order of this tab (ascending order for display)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Navigation property for custom fields in this tab
        /// </summary>
        public virtual ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();
    }
}
