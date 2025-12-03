using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventGiftRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn sự kiện")]
        public string EventId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên phần quà")]
        [StringLength(100, ErrorMessage = "Tên phần quà không được vượt quá 100 ký tự")]
        public string GiftName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        // File uploads
        public List<IFormFile>? ImageFiles { get; set; }
    }

    public class UpdateEventGiftRequest : CreateEventGiftRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
    }
}
