using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.ETM
{
    public class ZaloTemplateConfig : BaseEntity
    {
        public required string Recipients { get; set; }  // cấu hình những người nhận được tin này. trigger là người kích hoạt event này lấy UID nha
        public required string TemplateId { get; set; }
        public required string TemplateMapping { get; set; }
    }
}
