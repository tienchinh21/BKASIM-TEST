using MiniAppGIBA.Entities.Commons;
namespace MiniAppGIBA.Entities.Admins
{
    public class Roles : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}