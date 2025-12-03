
using System.ComponentModel.DataAnnotations;
namespace MiniAppGIBA.Entities.Commons
{
    public abstract class BaseEntity
    {
        [Key]
        [MaxLength(32)]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
