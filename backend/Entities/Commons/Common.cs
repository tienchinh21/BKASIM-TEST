using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.Commons
{
    public class SystemConfig : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class Common : BaseEntity
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
    }
}
