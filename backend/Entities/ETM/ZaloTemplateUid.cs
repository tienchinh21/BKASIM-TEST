using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class ZaloTemplateUid : BaseEntity
    {
        public required string Name { get; set; }
        public required string Message { get; set; }
        public string? ListParams { get; set; } // phân cách nhau bằng dấu, sử dụng cái này để replace cái mapping data sang nội dung template
    }
}
