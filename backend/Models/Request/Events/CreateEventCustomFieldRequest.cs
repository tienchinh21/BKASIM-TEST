using System.ComponentModel.DataAnnotations;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Models.Request.Events
{
    public class CreateEventCustomFieldRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn sự kiện")]
        public string EventId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên trường")]
        [StringLength(200, ErrorMessage = "Tên trường không được vượt quá 200 ký tự")]
        public string FieldName { get; set; } = string.Empty;

        public string FieldValue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn kiểu dữ liệu")]
        public EEventFieldType FieldType { get; set; }

        public bool IsRequired { get; set; } = false;
    }

    public class UpdateEventCustomFieldRequest : CreateEventCustomFieldRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập ID trường")]
        public string Id { get; set; } = string.Empty;
    }
}

