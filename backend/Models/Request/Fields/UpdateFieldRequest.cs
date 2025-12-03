using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Fields
{
    public class UpdateFieldRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lĩnh vực là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lĩnh vực không được vượt quá 100 ký tự")]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public int DisplayOrderMiniApp { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public List<UpdateFieldChildRequest> Children { get; set; } = new List<UpdateFieldChildRequest>();
    }

    public class UpdateFieldChildRequest
    {
        public string? Id { get; set; } // null = tạo mới, có value = update
        
        [Required(ErrorMessage = "Tên lĩnh vực con là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên lĩnh vực con không được vượt quá 255 ký tự")]
        public string ChildName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false; // đánh dấu xóa
    }
}
