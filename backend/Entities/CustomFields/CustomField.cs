using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    /// <summary>
    /// Represents a custom field definition within a tab or at the entity level
    /// </summary>
    public class CustomField : BaseEntity
    {
        /// <summary>
        /// The ID of the parent tab (nullable for flat fields like EventRegistration)
        /// </summary>
        public string? CustomFieldTabId { get; set; }

        /// <summary>
        /// The type of entity this field belongs to (e.g., GroupMembership, EventRegistration)
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the specific entity instance (e.g., EventId for EventRegistration)
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the field (e.g., "Full Name", "Email Address")
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The data type of the field (Text, Email, Date, Dropdown, etc.)
        /// </summary>
        public EEventFieldType FieldType { get; set; }

        /// <summary>
        /// JSON string containing options for Dropdown or MultipleChoice fields
        /// </summary>
        public string? FieldOptions { get; set; }

        /// <summary>
        /// Whether this field is required during form submission
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The display order of this field within its tab (ascending order for display)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Navigation property for the parent tab
        /// </summary>
        public virtual CustomFieldTab? CustomFieldTab { get; set; }

        /// <summary>
        /// Navigation property for submitted values of this field
        /// </summary>
        public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}