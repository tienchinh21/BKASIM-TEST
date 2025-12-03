using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Fields
{
    public class CreateFieldRequest
    {
        [Required(ErrorMessage = "Tên lĩnh vực là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lĩnh vực không được vượt quá 100 ký tự")]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public int DisplayOrderMiniApp { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public List<CreateFieldChildRequest> Children { get; set; } = new List<CreateFieldChildRequest>();
    }

    public class CreateFieldChildRequest
    {
        [Required(ErrorMessage = "Tên lĩnh vực con là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên lĩnh vực con không được vượt quá 255 ký tự")]
        public string ChildName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
    }
}
