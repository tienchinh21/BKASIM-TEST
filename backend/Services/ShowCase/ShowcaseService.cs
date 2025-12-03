using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Showcase;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Showcase;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Models.Request.Showcase;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
namespace MiniAppGIBA.Services.ShowCase
{
    public class ShowcaseService : IShowcaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Showcase> _showcaseRepository;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<GroupPermission> _groupPermissionRepository;
        private readonly ILogger<ShowcaseService> _logger;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<Membership> _membershipRepository;

        public ShowcaseService(IUnitOfWork unitOfWork, ILogger<ShowcaseService> logger)
        {
            _unitOfWork = unitOfWork;
            _showcaseRepository = unitOfWork.GetRepository<Showcase>();
            _rolesRepository = unitOfWork.GetRepository<Roles>();
            _groupRepository = unitOfWork.GetRepository<Group>();
            _groupPermissionRepository = unitOfWork.GetRepository<GroupPermission>();
            _membershipGroupRepository = unitOfWork.GetRepository<MembershipGroup>();
            _membershipRepository = unitOfWork.GetRepository<Membership>();
            _logger = logger;
        }

        public async Task<PagedResult<Showcase>> GetPage(ShowcaseQueryParams query, string roleId, string userId)
        {
            var queryable = _showcaseRepository.AsQueryable();
            var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(p => p.Id == roleId);
            var groupIds = new List<string>();
            
            // GIBA has full access - can see all showcases
            if (role != null && role.Name == CTRole.GIBA)
            {
                groupIds = await _groupRepository.AsQueryable().Select(g => g.Id).ToListAsync();
            }
            else if (string.IsNullOrEmpty(roleId) && string.IsNullOrEmpty(userId))
            {
                // Không đăng nhập: chỉ lấy public
                queryable = queryable.Where(p => p.IsPublic);
                groupIds = await _groupRepository.AsQueryable().Select(g => g.Id).ToListAsync();
            }
            else if (string.IsNullOrEmpty(roleId))
            {
                // Có đăng nhập: lấy public HOẶC nội bộ của nhóm user tham gia
                var user = await _membershipRepository.AsQueryable().FirstOrDefaultAsync(u => u.UserZaloId == userId);
                if (user != null)
                {
                    groupIds = await _membershipGroupRepository.AsQueryable().Where(mg => mg.UserZaloId == user.UserZaloId && mg.IsApproved == true).Select(mg => mg.GroupId).ToListAsync();
                }
            }

            if (groupIds.Count > 0)
            {
                // Có nhóm user tham gia: lấy public HOẶC nội bộ của nhóm user tham gia
                queryable = queryable.Where(p => p.IsPublic || groupIds.Contains(p.GroupId));
            }
            else
            {
                // Không có nhóm: chỉ lấy public
                queryable = queryable.Where(p => p.IsPublic);
            }

            if (query.GroupId != null)
            {
                queryable = queryable.Where(p => p.GroupId == query.GroupId);
            }
            if (!string.IsNullOrEmpty(query.AuthorName))
            {
                queryable = queryable.Where(p => p.MembershipName == query.AuthorName);
            }

            // Filter by status - calculate dynamically based on StartDate and EndDate
            if (query.Status.HasValue)
            {
                var now = DateTime.Now;
                var statusValue = query.Status.Value;
                if (statusValue == 1) // Đã lên lịch / Sắp diễn ra
                {
                    queryable = queryable.Where(p => p.StartDate > now);
                }
                else if (statusValue == 2) // Đang diễn ra
                {
                    queryable = queryable.Where(p => p.StartDate <= now && p.EndDate >= now);
                }
                else if (statusValue == 3) // Đã hoàn thành / Đã kết thúc
                {
                    queryable = queryable.Where(p => p.EndDate < now);
                }
                // Status 4 (Đã hủy) - giữ nguyên filter theo Status field vì đây là trạng thái đặc biệt
                else if (statusValue == 4)
                {
                    queryable = queryable.Where(p => p.Status == 4);
                }
            }

            if (query.IsPublic.HasValue)
            {
                queryable = queryable.Where(p => p.IsPublic == query.IsPublic.Value);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var keyword = query.Keyword.ToLower().Trim();
                queryable = queryable.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(keyword)) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                    (p.Location != null && p.Location.ToLower().Contains(keyword)) ||
                    (p.MembershipName != null && p.MembershipName.ToLower().Contains(keyword))
                );
            }

            if (query.StartDate != null)
            {
                queryable = queryable.Where(p => p.StartDate >= query.StartDate);
            }

            if (query.EndDate != null)
            {
                if (query.StartDate != null)
                {
                    queryable = queryable.Where(p => p.StartDate <= query.EndDate && p.EndDate >= query.StartDate);
                }
                else
                {
                    queryable = queryable.Where(p => p.EndDate <= query.EndDate);
                }
            }

            var totalItems = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

            var items = await queryable
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Showcase>()
            {
                Items = items,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<bool> CreateAsync(ShowcaseRequest request)
        {
            try
            {
                var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(p => p.Name == request.RoleName);
                if (role == null)
                {
                    throw new Exception("Role not found");
                }
                // Auto-calculate status based on StartDate and EndDate
                var status = GetShowcaseStatus(request.StartDate, request.EndDate);

                var showcase = new Showcase
                {
                    GroupId = request.GroupId,
                    GroupName = request.GroupName,
                    Title = request.Title,
                    Description = request.Description,
                    MembershipId = request.MembershipId,
                    MembershipName = request.MembershipName,
                    MemberShipAvatar = request.MemberShipAvatar ?? string.Empty,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Location = request.Location,
                    Status = status, // Auto-calculated based on dates
                    CreatedBy = request.CreatedBy,
                    RoleId = role.Id,
                    IsPublic = request.IsPublic
                };
                await _showcaseRepository.AddAsync(showcase);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating showcase with request: {@Request}", request);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string id, ShowcaseRequest request)
        {
            try
            {
                var showcase = await _showcaseRepository.FindByIdAsync(id);
                if (showcase == null)
                {
                    throw new Exception("Showcase not found");
                }

                var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(p => p.Name == request.RoleName);
                if (role == null)
                {
                    throw new Exception("Role not found");
                }

                // Auto-calculate status based on StartDate and EndDate
                var status = GetShowcaseStatus(request.StartDate, request.EndDate);

                // Update showcase properties
                showcase.GroupId = request.GroupId;
                showcase.GroupName = request.GroupName;
                showcase.Title = request.Title;
                showcase.Description = request.Description;
                showcase.MembershipId = request.MembershipId;
                showcase.MembershipName = request.MembershipName;
                showcase.MemberShipAvatar = request.MemberShipAvatar ?? string.Empty;
                showcase.StartDate = request.StartDate;
                showcase.EndDate = request.EndDate;
                showcase.Location = request.Location;
                showcase.Status = status; // Auto-calculated based on dates
                showcase.RoleId = role.Id;
                showcase.UpdatedDate = DateTime.Now;
                showcase.IsPublic = request.IsPublic;
                _showcaseRepository.Update(showcase);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating showcase {Id} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var showcase = await _showcaseRepository.FindByIdAsync(id);
                if (showcase == null)
                {
                    throw new Exception("Showcase not found");
                }

                // Delete the showcase
                _showcaseRepository.Delete(showcase);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting showcase {Id}", id);
                throw;
            }
        }

        public async Task<Showcase?> GetByIdAsync(string id)
        {
            return await _showcaseRepository.FindByIdAsync(id);
        }

        private byte GetShowcaseStatus(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;
            if (now < startDate)
                return 1; // Đã lên lịch / Sắp diễn ra
            else if (now >= startDate && now <= endDate)
                return 2; // Đang diễn ra
            else
                return 3; // Đã hoàn thành / Đã kết thúc
            // Status 4 (Đã hủy) - chỉ set thủ công, không tự động tính
        }
    }
}