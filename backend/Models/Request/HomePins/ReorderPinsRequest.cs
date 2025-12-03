using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.HomePins
{
    /// <summary>
    /// Request model for reordering pins
    /// </summary>
    public class ReorderPinsRequest
    {
        [Required(ErrorMessage = "Pin ID is required")]
        public string PinId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New display order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Display order must be greater than 0")]
        public int NewDisplayOrder { get; set; }
    }
}
