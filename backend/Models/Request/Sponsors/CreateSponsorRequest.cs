using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Sponsors
{
    public class CreateSponsorRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhà tài trợ")]
        [StringLength(100, ErrorMessage = "Tên nhà tài trợ không được vượt quá 100 ký tự")]
        public string SponsorName { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Giới thiệu không được vượt quá 2000 ký tự")]
        public string? Introduction { get; set; }

        [Url(ErrorMessage = "URL website không hợp lệ")]
        public string? WebsiteURL { get; set; }

        public bool IsActive { get; set; } = true;

        // File upload
        public IFormFile? Image { get; set; }
    }

    public class UpdateSponsorRequest : CreateSponsorRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Flag to indicate if existing image should be removed
        /// </summary>
        public bool ShouldRemoveImage { get; set; } = false;
    }
}
