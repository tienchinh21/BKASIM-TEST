using MiniAppGIBA.Entities.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAppGIBA.Entities.Admins
{
    /// <summary>
    /// Bảng phân quyền nhóm cho ADMIN
    /// ADMIN có thể quản lý nhiều nhóm (Groups)
    /// </summary>
    public class GroupPermission : BaseEntity
    {

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái active của permission
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

