using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Entities.ETM
{
    public class DatasourceETM : BaseEntity
    {
        [MaxLength(50)]
        public required string Code { get; set; }
        [MaxLength(50)]
        public required string Key { get; set; }
        [MaxLength(200)]
        public required string Value { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
