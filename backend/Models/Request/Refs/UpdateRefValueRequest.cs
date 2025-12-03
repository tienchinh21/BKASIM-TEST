using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Refs
{
    public class UpdateRefValueRequest
    {
        // [Required(ErrorMessage = "Giá trị đơn hàng là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public double Value { get; set; }

        // ✨ Rating & Feedback fields
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public byte? Rating { get; set; }  // Optional

        [StringLength(500, ErrorMessage = "Nhận xét không được vượt quá 500 ký tự")]
        public string? Feedback { get; set; }  // Optional
    }
}
