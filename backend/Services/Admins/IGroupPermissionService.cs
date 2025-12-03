using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Models.DTOs.Admins;

namespace MiniAppGIBA.Services.Admins
{
    public interface IGroupPermissionService
    {
        /// <summary>
        /// Lấy danh sách GroupPermissions với thông tin join
        /// </summary>
        Task<List<GroupPermissionDto>> GetAllWithDetailsAsync();

        /// <summary>
        /// Lấy danh sách nhóm mà ADMIN được phân quyền
        /// </summary>
        Task<List<string>> GetGroupIdsByUserIdAsync(string userId);

        /// <summary>
        /// Lấy danh sách GroupPermissions của ADMIN
        /// </summary>
        Task<List<GroupPermissionDto>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Phân quyền nhóm cho ADMIN
        /// </summary>
        Task<GroupPermission> AssignGroupToAdminAsync(CreateGroupPermissionDto dto);

        /// <summary>
        /// Thu hồi quyền nhóm của ADMIN
        /// </summary>
        Task<bool> RevokeGroupFromAdminAsync(string permissionId);

        /// <summary>
        /// Xóa hoàn toàn permission
        /// </summary>
        Task<bool> DeletePermissionAsync(string permissionId);

        /// <summary>
        /// Kiểm tra ADMIN có quyền truy cập nhóm không
        /// </summary>
        Task<bool> HasGroupPermissionAsync(string userId, string groupId);
    }
}

