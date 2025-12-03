using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Entities.CustomFields
{
    /// <summary>
    /// Represents a submitted value for a custom field
    /// </summary>
    public class CustomFieldValue : BaseEntity
    {
        /// <summary>
        /// The ID of the custom field definition
        /// </summary>
        public string CustomFieldId { get; set; } = string.Empty;

        /// <summary>
        /// The type of entity this value belongs to (e.g., GroupMembership, EventRegistration)
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the specific entity instance (e.g., MembershipGroupId for membership submission)
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the field (archived field name for deleted fields)
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The actual value submitted by the user
        /// </summary>
        public string FieldValue { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property for the custom field definition
        /// </summary>
        public virtual CustomField CustomField { get; set; } = null!;
    }
}
