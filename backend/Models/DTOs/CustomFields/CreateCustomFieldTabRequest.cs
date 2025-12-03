using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Request model for creating a new custom field tab
    /// </summary>
    public class CreateCustomFieldTabRequest
    {
        /// <summary>
        /// The type of entity this tab belongs to
        /// </summary>
        public ECustomFieldEntityType EntityType { get; set; }

        /// <summary>
        /// The ID of the entity instance (e.g., GroupId)
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the tab
        /// </summary>
        public string TabName { get; set; } = string.Empty;

        /// <summary>
        /// The display order of this tab
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
