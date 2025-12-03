using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Models.DTOs.Admins;

namespace MiniAppGIBA.Services.Admins
{
    public class GroupPermissionService : Service<GroupPermission>, IGroupPermissionService
    {
        public GroupPermissionService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Lấy danh sách GroupPermissions với thông tin join (KHÔNG dùng navigation)
        /// </summary>
        public async Task<List<GroupPermissionDto>> GetAllWithDetailsAsync()
        {
            var permissions = await _repository.AsQueryable().ToListAsync();

            var result = new List<GroupPermissionDto>();

            foreach (var perm in permissions)
            {
                // Join thủ công bằng FK (KHÔNG dùng navigation)
                var user = await unitOfWork.Context.Users.FindAsync(perm.UserId);
                var group = await unitOfWork.Context.Groups.FindAsync(perm.GroupId);

                result.Add(new GroupPermissionDto
                {
                    Id = perm.Id,
                    UserId = perm.UserId,
                    GroupId = perm.GroupId,
                    IsActive = perm.IsActive,
                    CreatedDate = perm.CreatedDate,
                    UpdatedDate = perm.UpdatedDate,
                    UserFullName = user?.FullName,
                    UserEmail = user?.Email,
                    GroupName = group?.GroupName
                });
            }

            return result;
        }

        /// <summary>
        /// Lấy danh sách nhóm mà ADMIN được phân quyền
        /// </summary>
        public async Task<List<string>> GetGroupIdsByUserIdAsync(string userId)
        {
            return await _repository.AsQueryable()
                .Where(p => p.UserId == userId && p.IsActive)
                .Select(p => p.GroupId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách GroupPermissions của ADMIN
        /// </summary>
        public async Task<List<GroupPermissionDto>> GetByUserIdAsync(string userId)
        {
            var permissions = await _repository.AsQueryable()
                .Where(p => p.UserId == userId && p.IsActive)
                .ToListAsync();

            var result = new List<GroupPermissionDto>();
            foreach (var perm in permissions)
            {
                result.Add(new GroupPermissionDto
                {
                    Id = perm.Id,
                    UserId = perm.UserId,
                    GroupId = perm.GroupId,
                    IsActive = perm.IsActive,
                    CreatedDate = perm.CreatedDate,
                    UpdatedDate = perm.UpdatedDate
                });
            }

            return result;
        }

        /// <summary>
        /// Phân quyền nhóm cho ADMIN
        /// </summary>
        public async Task<GroupPermission> AssignGroupToAdminAsync(CreateGroupPermissionDto dto)
        {
            // Kiểm tra đã tồn tại chưa
            var existing = await _repository.AsQueryable()
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.GroupId == dto.GroupId);

            if (existing != null)
            {
                // Nếu đã tồn tại nhưng IsActive = false, thì active lại
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.UpdatedDate = DateTime.Now;
                    _repository.Update(existing);
                    await unitOfWork.SaveChangesAsync();
                }
                return existing;
            }

            // Tạo mới
            var permission = new GroupPermission
            {
                UserId = dto.UserId,
                GroupId = dto.GroupId,
                IsActive = true
            };

            await _repository.AddAsync(permission);
            await unitOfWork.SaveChangesAsync();

            return permission;
        }

        /// <summary>
        /// Thu hồi quyền nhóm của ADMIN
        /// </summary>
        public async Task<bool> RevokeGroupFromAdminAsync(string permissionId)
        {
            var permission = await _repository.FindByIdAsync(permissionId);
            if (permission == null) return false;

            permission.IsActive = false;
            permission.UpdatedDate = DateTime.Now;

            _repository.Update(permission);
            await unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Xóa hoàn toàn permission
        /// </summary>
        public async Task<bool> DeletePermissionAsync(string permissionId)
        {
            var permission = await _repository.FindByIdAsync(permissionId);
            if (permission == null) return false;

            _repository.Delete(permission);
            await unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Kiểm tra ADMIN có quyền truy cập nhóm không
        /// </summary>
        public async Task<bool> HasGroupPermissionAsync(string userId, string groupId)
        {
            return await _repository.AsQueryable()
                .AnyAsync(p => p.UserId == userId && p.GroupId == groupId && p.IsActive);
        }
    }
}

