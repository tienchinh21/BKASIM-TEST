using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Memberships;
using MiniAppGIBA.Models.Queries.Memberships;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Entities.Admins;

namespace MiniAppGIBA.Services.Memberships
{
    public class MembershipService : IMembershipService
    {
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MembershipService> _logger;

        public MembershipService(
            IRepository<Membership> membershipRepository,
            IRepository<MembershipGroup> membershipGroupRepository,
            IRepository<Roles> rolesRepository,
            IUnitOfWork unitOfWork,
            ILogger<MembershipService> logger)
        {
            _membershipRepository = membershipRepository;
            _membershipGroupRepository = membershipGroupRepository;
            _rolesRepository = rolesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MembershipDTO> CreateMembershipAsync(CreateMembershipRequest request)
        {
            try
            {
                var existingMembership = await _membershipRepository.GetFirstOrDefaultAsync(m => m.PhoneNumber == request.PhoneNumber && m.IsDelete != true);
                if (existingMembership != null)
                {
                    throw new CustomException(400, "Số điện thoại đã tồn tại trong hệ thống");
                }

                var userZaloId = request.UserZaloId ?? Guid.NewGuid().ToString("N");
                var slug = !string.IsNullOrEmpty(request.Fullname)
                    ? await GenerateUniqueSlugAsync(request.Fullname)
                    : request.PhoneNumber;

                var membership = new Membership
                {
                    UserZaloId = userZaloId,
                    UserZaloName = request.UserZaloName,
                    PhoneNumber = request.PhoneNumber,
                    Fullname = request.Fullname,
                    Slug = slug,
                    ZaloAvatar = request.ZaloAvatar,
                    RoleId = request.RoleId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _membershipRepository.AddAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                return MapToDTO(membership);
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership");
                throw new CustomException(500, "Có lỗi xảy ra khi tạo tài khoản");
            }
        }


        public async Task<PagedResult<MembershipDTO>> GetMembershipsAsync(MembershipQueryParameters queryParameters, List<string>? allowedGroupIds = null)
        {
            try
            {
                var query = _membershipRepository.AsQueryable().Where(m => m.IsDelete != true);

                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var userZaloIdsInAllowedGroups = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => allowedGroupIds.Contains(mg.GroupId) && mg.IsApproved == true)
                        .Select(mg => mg.UserZaloId)
                        .Distinct()
                        .ToListAsync();
                    query = query.Where(m => userZaloIdsInAllowedGroups.Contains(m.UserZaloId));
                }

                if (!string.IsNullOrEmpty(queryParameters.Keyword))
                {
                    query = query.Where(m =>
                        (m.Fullname != null && m.Fullname.Contains(queryParameters.Keyword)) ||
                        (m.PhoneNumber != null && m.PhoneNumber.Contains(queryParameters.Keyword)));
                }

                query = queryParameters.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(GetSortExpression(queryParameters.SortBy))
                    : query.OrderByDescending(GetSortExpression(queryParameters.SortBy));

                var totalItems = await query.CountAsync();
                var memberships = await query
                    .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                    .Take(queryParameters.PageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalItems / (double)queryParameters.PageSize);
                return new PagedResult<MembershipDTO>
                {
                    Items = memberships.Select(MapToDTO).ToList(),
                    TotalItems = totalItems,
                    Page = queryParameters.Page,
                    PageSize = queryParameters.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memberships");
                throw;
            }
        }

        public async Task<MembershipDTO?> GetMembershipByIdAsync(string id)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDelete != true);
            return membership != null ? MapToDTO(membership) : null;
        }

        public async Task<MembershipDTO?> GetMembershipByPhoneAsync(string phone)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.PhoneNumber == phone && m.IsDelete != true);
            return membership != null ? MapToDTO(membership) : null;
        }

        public async Task<MembershipDTO?> GetMembershipByUserZaloIdAsync(string userZaloId)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);
            return membership != null ? MapToDTO(membership) : null;
        }

        public async Task<MembershipDTO> UpdateMembershipAsync(string id, UpdateMembershipRequest request)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDelete != true);
            if (membership == null)
                throw new CustomException(404, "Không tìm thấy thành viên!");

            membership.Fullname = request.Fullname;
            if (request.UserZaloName != null) membership.UserZaloName = request.UserZaloName;
            if (request.ZaloAvatar != null) membership.ZaloAvatar = request.ZaloAvatar;
            if (request.PhoneNumber != null) membership.PhoneNumber = request.PhoneNumber;
            if (request.RoleId != null) membership.RoleId = request.RoleId;
            membership.UpdatedDate = DateTime.Now;

            _membershipRepository.Update(membership);
            await _unitOfWork.SaveChangesAsync();
            return MapToDTO(membership);
        }

        public async Task<bool> DeleteMembershipAsync(string id)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDelete != true);
            if (membership == null)
                throw new CustomException(404, "Không tìm thấy thành viên!");

            membership.IsDelete = true;
            membership.UpdatedDate = DateTime.Now;
            _membershipRepository.Update(membership);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<MembershipDTO>> GetActiveMembershipsAsync(List<string>? allowedGroupIds = null)
        {
            var query = _membershipRepository.AsQueryable().Where(m => m.IsDelete != true);

            if (allowedGroupIds != null && allowedGroupIds.Any())
            {
                var userZaloIdsInAllowedGroups = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => allowedGroupIds.Contains(mg.GroupId) && mg.IsApproved == true)
                    .Select(mg => mg.UserZaloId)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(m => userZaloIdsInAllowedGroups.Contains(m.UserZaloId));
            }

            var memberships = await query.OrderBy(m => m.Fullname).ToListAsync();
            return memberships.Select(MapToDTO).ToList();
        }

        public async Task<List<MembershipDTO>> SearchMembersForGroupAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<MembershipDTO>();

            var memberships = await _membershipRepository.AsQueryable()
                .Where(m => m.IsDelete != true && (m.Fullname.Contains(searchTerm) || m.PhoneNumber.Contains(searchTerm)))
                .Take(10)
                .ToListAsync();
            return memberships.Select(MapToDTO).ToList();
        }


        public async Task<List<MembershipDTO>> GetMembershipsByRoleAsync(string roleName)
        {
            var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) throw new CustomException(404, "Không tìm thấy vai trò!");

            var memberships = await _membershipRepository.AsQueryable()
                .Where(m => m.RoleId == role.Id && m.IsDelete != true)
                .ToListAsync();
            return memberships.Select(MapToDTO).ToList();
        }

        public async Task<string> GetRoleNameAsync(string roleId)
        {
            var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(r => r.Id == roleId);
            return role?.Name ?? string.Empty;
        }

        public async Task<MembershipDTO> CreateAdminMembershipAsync(string fullName, string username, string password, string phoneNumber, string roleName)
        {
            var existingPhone = await GetMembershipByPhoneAsync(phoneNumber);
            if (existingPhone != null)
                throw new CustomException(400, "Số điện thoại đã tồn tại trong hệ thống");

            var existingUsername = await _membershipRepository.GetFirstOrDefaultAsync(m => m.Username == username && m.IsDelete != true);
            if (existingUsername != null)
                throw new CustomException(400, "Tên đăng nhập đã tồn tại trong hệ thống");

            var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new CustomException(404, $"Không tìm thấy vai trò: {roleName}");

            if (string.IsNullOrWhiteSpace(password))
                throw new CustomException(400, "Mật khẩu không được để trống");

            var membership = new Membership
            {
                UserZaloId = Guid.NewGuid().ToString("N"),
                UserZaloName = fullName,
                Fullname = fullName,
                Username = username,
                Password = AuthHelper.HashPassword(password),
                PhoneNumber = phoneNumber,
                RoleId = role.Id,
                Slug = username.ToLower().Replace(" ", "-"),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _membershipRepository.AddAsync(membership);
            await _unitOfWork.SaveChangesAsync();
            return MapToDTO(membership);
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == userId && m.IsDelete != true);
            if (membership == null)
                throw new CustomException(404, "Không tìm thấy tài khoản");

            membership.Password = AuthHelper.HashPassword(newPassword);
            membership.UpdatedDate = DateTime.Now;
            _membershipRepository.Update(membership);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateUniqueSlugAsync(string fullname)
        {
            var baseSlug = SlugGenerator.ToSlug(fullname);
            if (string.IsNullOrEmpty(baseSlug) || SlugGenerator.IsReservedSlug(baseSlug))
                baseSlug = $"user-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            var slug = baseSlug;
            var counter = 1;
            while (await _membershipRepository.AsQueryable().AnyAsync(m => m.Slug == slug && m.IsDelete != true))
            {
                slug = SlugGenerator.AppendNumber(baseSlug, counter++);
                if (counter > 1000)
                {
                    slug = $"{baseSlug}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    break;
                }
            }
            return slug;
        }

        private static MembershipDTO MapToDTO(Membership m) => new MembershipDTO
        {
            Id = m.Id,
            UserZaloId = m.UserZaloId,
            UserZaloName = m.UserZaloName,
            UserZaloIdByOA = m.UserZaloIdByOA,
            Fullname = m.Fullname,
            Username = m.Username,
            Slug = m.Slug,
            OldSlugs = m.OldSlugs,
            ZaloAvatar = m.ZaloAvatar,
            PhoneNumber = m.PhoneNumber,
            RoleId = m.RoleId,
            IsDelete = m.IsDelete,
            CreatedDate = m.CreatedDate,
            UpdatedDate = m.UpdatedDate
        };

        private static System.Linq.Expressions.Expression<Func<Membership, object>> GetSortExpression(string? sortBy) =>
            sortBy?.ToLower() switch
            {
                "fullname" => m => m.Fullname,
                "phone" => m => m.PhoneNumber,
                "updatedate" => m => m.UpdatedDate,
                _ => m => m.CreatedDate
            };
    }
}
