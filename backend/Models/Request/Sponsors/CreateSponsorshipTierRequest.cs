using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Sponsors
{
    public class CreateSponsorshipTierRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên hạng tài trợ")]
        [StringLength(100, ErrorMessage = "Tên hạng tài trợ không được vượt quá 100 ký tự")]
        public string TierName { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // File upload
        public IFormFile? Image { get; set; }
    }

    public class UpdateSponsorshipTierRequest : CreateSponsorshipTierRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Flag to indicate if existing image should be removed
        /// </summary>
        public bool ShouldRemoveImage { get; set; } = false;
    }
}
