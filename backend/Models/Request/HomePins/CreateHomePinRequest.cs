using System.ComponentModel.DataAnnotations;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.HomePins
{
    /// <summary>
    /// Request model for creating a new pin
    /// </summary>
    public class CreateHomePinRequest
    {
        [Required(ErrorMessage = "Entity type is required")]
        public PinEntityType EntityType { get; set; }
        
        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(32, ErrorMessage = "Entity ID cannot exceed 32 characters")]
        public string EntityId { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
