using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Enum;
namespace MiniAppGIBA.Service.Groups
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<GuestList> _guestListRepository;
        private readonly IRepository<MiniAppGIBA.Entities.Memberships.Membership> _membershipRepository;
        private readonly ILogger<GroupService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public GroupService(
            IUnitOfWork unitOfWork,
            ILogger<GroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _groupRepository = unitOfWork.GetRepository<Group>();
            _membershipGroupRepository = unitOfWork.GetRepository<MembershipGroup>();
            _guestListRepository = unitOfWork.GetRepository<GuestList>();
            _membershipRepository = unitOfWork.GetRepository<MiniAppGIBA.Entities.Memberships.Membership>();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        /// <summary>
        /// Lấy danh sách hội nhóm với phân trang và filter
        /// </summary>
        public async Task<PagedResult<GroupDTO>> GetGroupsAsync(GroupQueryParameters query, string? userZaloId = null, List<string>? allowedGroupIds = null)
        {
            try
            {

                IQueryable<Group> queryable = _groupRepository.AsQueryable()
                    .Include(g => g.MembershipGroups.Where(mg => mg.Membership != null && mg.Membership.IsDelete != true))
                    .ThenInclude(mg => mg.Membership);

                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    queryable = queryable.Where(g => allowedGroupIds.Contains(g.Id));
                }

                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(g =>
                        g.GroupName.Contains(query.Keyword) ||
                        (g.Description != null && g.Description.Contains(query.Keyword)));
                }

                if (query.Status.HasValue)
                {
                    queryable = queryable.Where(g => g.IsActive == query.Status.Value);
                }

                // Filter by specific group ID
                if (!string.IsNullOrEmpty(query.GroupId))
                {
                    queryable = queryable.Where(g => g.Id == query.GroupId);
                }

                // Lấy danh sách group IDs mà user đã tham gia (nếu có userZaloId)
                var joinedGroupIds = new HashSet<string>();
                var pendingGroupIds = new HashSet<string>();
                if (!string.IsNullOrEmpty(userZaloId))
                {
                    // Lấy groups đã được duyệt
                    joinedGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();

                    // Lấy groups đang chờ duyệt để loại bỏ khỏi kết quả
                    pendingGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == null)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();

                    // Loại bỏ các groups đã tham gia và đang chờ duyệt
                    var excludedGroupIds = joinedGroupIds.Union(pendingGroupIds);
                    queryable = queryable.Where(g => !excludedGroupIds.Contains(g.Id));
                }
                else
                {
                    // Nếu không có userZaloId, joinedGroupIds sẽ rỗng (tất cả nhóm đều IsJoined = false)
                    joinedGroupIds = new HashSet<string>();
                }

                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(g => g.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();
                // Console.WriteLine("dbasjkdadasdasbdjkas");
                var groupDTOs = items.Select(g => new GroupDTO
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Rule = g.Rule,
                    IsActive = g.IsActive,
                    Logo = GetFullUrl(g.Logo),
                    CreatedDate = g.CreatedDate,
                    UpdatedDate = g.UpdatedDate,
                    MemberCount = g.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0,
                    IsJoined = joinedGroupIds.Contains(g.Id),
                    MainActivities = g.MainActivities
                }).ToList();

                return new PagedResult<GroupDTO>
                {
                    Items = groupDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups with query: {@Query}", query);
                throw;
            }
        }
        public async Task<PagedResult<GroupDTO>> GuestGetPage(string? type, bool? isActive, int page, int pageSize)
        {
            try
            {
                // Build query with Include để load MembershipGroups và Membership
                IQueryable<Group> queryable = _groupRepository.AsQueryable()
                    .Where(g => g.IsActive)
                    .Include(g => g.MembershipGroups)
                        .ThenInclude(mg => mg.Membership);
                // type parameter is deprecated - no longer filtering by group type

                // Filter by status
                if (isActive.HasValue)
                {
                    queryable = queryable.Where(g => g.IsActive == isActive.Value);
                }
                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(g => g.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var groupDTOs = items.Select(g => new GroupDTO
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Rule = g.Rule,
                    IsActive = g.IsActive,
                    Logo = GetFullUrl(g.Logo),
                    CreatedDate = g.CreatedDate,
                    UpdatedDate = g.UpdatedDate,
                    MemberCount = g.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0,
                    IsJoined = false,
                    MainActivities = g.MainActivities
                }).ToList();

                return new PagedResult<GroupDTO>
                {
                    Items = groupDTOs,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups with query: {@Query}", new { type, isActive });
                throw;
            }
        }
        /// <summary>
        /// Lấy tất cả hội nhóm với phân trang - hiển thị tất cả groups, isJoined dựa trên việc user đã tham gia hay chưa
        /// </summary>
        public async Task<PagedResult<GroupDTO>> GetAllGroupsAsync(GroupQueryParameters query, string? userZaloId = null, List<string>? allowedGroupIds = null)
        {
            try
            {
                // Build query with Include để load MembershipGroups và Membership
                IQueryable<Group> queryable = _groupRepository.AsQueryable()
                    .Where(g => g.IsActive)
                    .Include(g => g.MembershipGroups)
                        .ThenInclude(mg => mg.Membership);

                // GroupType filtering is deprecated - GIBA has full access to all groups

                // Filter by allowed group IDs (deprecated - GIBA has full access)
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    queryable = queryable.Where(g => allowedGroupIds.Contains(g.Id));
                }

                // Apply filters
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(g =>
                        g.GroupName.Contains(query.Keyword) ||
                        (g.Description != null && g.Description.Contains(query.Keyword)));
                }

                if (query.IsActive.HasValue)
                {
                    queryable = queryable.Where(g => g.IsActive == query.IsActive.Value);
                }
                var joinedGroupIds = new HashSet<string>();
                var pendingGroupIds = new HashSet<string>();
                var rejectedGroupIds = new HashSet<string>();
                if (!string.IsNullOrEmpty(userZaloId))
                {
                    Console.WriteLine("userZaloId: " + userZaloId);
                    // Lấy groups đã được duyệt
                    joinedGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();
                    Console.WriteLine("joinedGroupIds: " + joinedGroupIds.Count);
                    // Lấy groups đang chờ duyệt
                    pendingGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == null)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();
                    Console.WriteLine("pendingGroupIds: " + pendingGroupIds.Count);
                    rejectedGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == false)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();
                    Console.WriteLine("rejectedGroupIds: " + rejectedGroupIds.Count);
                }

                var allItems = await queryable
                    .OrderByDescending(g => g.CreatedDate)
                    .ToListAsync();

                var filteredItems = allItems.Where(g =>
                {
                    if (string.IsNullOrEmpty(query.JoinStatus))
                        return true; // Không filter

                    if (joinedGroupIds.Contains(g.Id))
                        return query.JoinStatus == "approved";
                    else if (pendingGroupIds.Contains(g.Id))
                        return query.JoinStatus == "pending";
                    else if (rejectedGroupIds.Contains(g.Id))
                        return query.JoinStatus == "rejected";
                    else
                        return query.JoinStatus == "available"; // Chưa tham gia
                }).ToList();

                // Count total items sau khi filter
                var totalItems = filteredItems.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply pagination
                var items = filteredItems
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();

                var groupDTOs = items.Select(g =>
                {
                    // Xác định trạng thái tham gia
                    string? joinStatus = null;
                    string? joinStatusText = null;
                    bool isJoined = false;

                    if (joinedGroupIds.Contains(g.Id))
                    {
                        joinStatus = "approved";
                        joinStatusText = "Đã tham gia";
                        isJoined = true;
                    }
                    else if (pendingGroupIds.Contains(g.Id))
                    {
                        joinStatus = "pending";
                        joinStatusText = "Chờ phê duyệt";
                        isJoined = false;
                    }
                    else if (rejectedGroupIds.Contains(g.Id))
                    {
                        joinStatus = "rejected";
                        joinStatusText = "Bị từ chối";
                        isJoined = false;
                    }

                    return new GroupDTO
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description,
                        Rule = g.Rule,
                        IsActive = g.IsActive,
                        Logo = GetFullUrl(g.Logo),
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate,
                        MemberCount = g.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0,
                        IsJoined = isJoined,
                        JoinStatus = joinStatus,
                        JoinStatusText = joinStatusText,
                        MainActivities = g.MainActivities
                    };
                }).ToList();

                return new PagedResult<GroupDTO>
                {
                    Items = groupDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all groups with query: {@Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết hội nhóm
        /// </summary>
        public async Task<GroupDetailDTO?> GetGroupByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("GetGroupByIdAsync called with ID: {GroupId}", id);

                var group = await _groupRepository.AsQueryable()
                    .Include(g => g.MembershipGroups.Where(mg => mg.Membership != null && mg.Membership.IsDelete != true))
                        .ThenInclude(mg => mg.Membership)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (group == null)
                {
                    _logger.LogWarning("Group not found in database with ID: {GroupId}", id);
                    return null;
                }

                _logger.LogInformation("Group found in database: {GroupName}, IsActive: {IsActive}, Description: {Description}",
                    group.GroupName, group.IsActive, group.Description);

                return new GroupDetailDTO
                {
                    Id = group.Id,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Rule = group.Rule,
                    IsActive = group.IsActive,
                    Logo = GetFullUrl(group.Logo),
                    CreatedDate = group.CreatedDate,
                    UpdatedDate = group.UpdatedDate,
                    MemberCount = group.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0,
                    MainActivities = group.MainActivities,
                    Members = group.MembershipGroups?
                        .Where(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) // Chỉ lấy thành viên đã được duyệt và chưa bị xóa
                        .Select(mg => new GroupMemberDTO
                        {
                            MembershipId = mg.Id,
                            MembershipName = mg.Membership?.Fullname ?? "N/A",
                            PhoneNumber = mg.Membership?.PhoneNumber ?? "N/A",
                            JoinedDate = mg.CreatedDate,
                            IsApproved = mg.IsApproved ?? false,
                            HasCustomFieldsSubmitted = mg.HasCustomFieldsSubmitted
                        }).ToList() ?? new List<GroupMemberDTO>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group by id: {GroupId}", id);
                throw;
            }
        }

        /// <summary>
        /// Tạo hội nhóm mới
        /// </summary>
        public async Task<GroupDTO> CreateGroupAsync(CreateGroupRequest request)
        {
            try
            {
                string? logo = null;
                if (request.Logo != null)
                {
                    var savePath = Path.Combine(_env.WebRootPath, "uploads", "images", "group-logos");
                    var fileName = await FileHandler.SaveFile(request.Logo, savePath);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new InvalidOperationException("Không thể tải lên logo");
                    }
                    // Lưu relative path vào database
                    logo = $"/uploads/images/group-logos/{fileName}";
                }
                // Kiểm tra tên hội nhóm đã tồn tại chưa
                var existingGroup = await _groupRepository.GetFirstOrDefaultAsync(g => g.GroupName == request.GroupName);
                if (existingGroup != null)
                {
                    throw new InvalidOperationException("Tên hội nhóm đã tồn tại");
                }

                var group = new Group
                {
                    GroupName = request.GroupName,
                    Description = request.Description,
                    Rule = request.Rule,
                    IsActive = request.IsActive, // Sử dụng giá trị từ form
                    Logo = logo,
                    MainActivities = request.MainActivities,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };


                await _groupRepository.AddAsync(group);

                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Không thể lưu hội nhóm vào database - SaveChanges returned 0 rows");
                }


                return new GroupDTO
                {
                    Id = group.Id,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Rule = group.Rule,
                    IsActive = group.IsActive,
                    Logo = GetFullUrl(group.Logo),
                    CreatedDate = group.CreatedDate,
                    UpdatedDate = group.UpdatedDate,
                    MemberCount = 0,
                    MainActivities = group.MainActivities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin hội nhóm
        /// </summary>
        public async Task<GroupDTO> UpdateGroupAsync(string id, UpdateGroupRequest request)
        {
            try
            {

                var group = await _groupRepository.AsQueryable()
                    .Include(g => g.MembershipGroups)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (group == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy hội nhóm");
                }

                // Kiểm tra tên hội nhóm đã tồn tại chưa (trừ chính nó)
                var existingGroup = await _groupRepository.GetFirstOrDefaultAsync(g => g.GroupName == request.GroupName && g.Id != id);
                if (existingGroup != null)
                {
                    throw new InvalidOperationException("Tên hội nhóm đã tồn tại");
                }
                string? logo = null;
                if (request.Logo != null)
                {
                    // Xóa logo cũ nếu có
                    if (!string.IsNullOrEmpty(group.Logo))
                    {
                        var oldLogoPath = Path.Combine(_env.WebRootPath, group.Logo.TrimStart('/'));
                        FileHandler.DeleteFile(oldLogoPath);
                    }
                    // Lưu logo mới
                    var savePath = Path.Combine(_env.WebRootPath, "uploads", "images", "group-logos");
                    var fileName = await FileHandler.SaveFile(request.Logo, savePath);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new InvalidOperationException("Không thể tải lên logo");
                    }
                    // Lưu relative path vào database
                    logo = $"/uploads/images/group-logos/{fileName}";
                }
                group.GroupName = request.GroupName;
                group.Description = request.Description;
                group.Rule = request.Rule;
                group.IsActive = request.IsActive;
                group.MainActivities = request.MainActivities;
                if (logo != null)
                {
                    group.Logo = logo;
                }
                group.UpdatedDate = DateTime.Now;

                _groupRepository.Update(group);
                await _unitOfWork.SaveChangesAsync();

                return new GroupDTO
                {
                    Id = group.Id,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Rule = group.Rule,
                    IsActive = group.IsActive,
                    Logo = GetFullUrl(group.Logo),
                    CreatedDate = group.CreatedDate,
                    UpdatedDate = group.UpdatedDate,
                    MemberCount = group.MembershipGroups?.Count(mg => mg.IsApproved == true) ?? 0,
                    MainActivities = group.MainActivities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId} with request: {@Request}", id, request);
                throw;
            }
        }

        /// <summary>
        /// Xóa hội nhóm
        /// </summary>
        public async Task<bool> DeleteGroupAsync(string id)
        {
            try
            {
                var group = await _groupRepository.AsQueryable()
                    .Include(g => g.MembershipGroups)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (group == null)
                {
                    return false;
                }

                // Kiểm tra xem hội nhóm có thành viên đã được duyệt không
                if (group.MembershipGroups?.Any(mg => mg.IsApproved == true) == true)
                {
                    throw new InvalidOperationException("Không thể xóa hội nhóm có thành viên đã được duyệt");
                }

                // Xóa logo nếu có
                if (!string.IsNullOrEmpty(group.Logo))
                {
                    try
                    {
                        var logoPath = Path.Combine(_env.WebRootPath, group.Logo.TrimStart('/'));
                        FileHandler.DeleteFile(logoPath);
                        _logger.LogInformation("Deleted logo file for group {GroupId}: {LogoPath}", id, logoPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete logo file for group {GroupId}: {LogoPath}", id, group.Logo);
                        // Không throw exception, chỉ log warning để không block việc xóa group
                    }
                }

                _groupRepository.Delete(group);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupId}", id);
                throw;
            }
        }

        /// <summary>
        /// Thay đổi trạng thái hội nhóm
        /// </summary>
        public async Task<bool> ToggleGroupStatusAsync(string id)
        {
            try
            {
                var group = await _groupRepository.FindByIdAsync(id);
                if (group == null)
                {
                    return false;
                }

                group.IsActive = !group.IsActive;
                group.UpdatedDate = DateTime.Now;

                _groupRepository.Update(group);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling group status {GroupId}", id);
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra tên hội nhóm có tồn tại không
        /// </summary>
        public async Task<bool> IsGroupNameExistsAsync(string name, string? excludeId = null)
        {
            try
            {
                var query = _groupRepository.GetQueryable().Where(g => g.GroupName == name);

                if (!string.IsNullOrEmpty(excludeId))
                {
                    query = query.Where(g => g.Id != excludeId);
                }

                return await _groupRepository.AnyAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking group name exists: {GroupName}", name);
                throw;
            }
        }

        /// <summary>
        /// Lấy thống kê hội nhóm
        /// </summary>
        public async Task<GroupStatisticsDTO> GetGroupStatisticsAsync()
        {
            try
            {
                var totalGroups = await _groupRepository.CountAsync();
                var activeGroups = await _groupRepository.CountAsync(g => g.IsActive);
                var inactiveGroups = totalGroups - activeGroups;

                return new GroupStatisticsDTO
                {
                    TotalGroups = totalGroups,
                    ActiveGroups = activeGroups,
                    InactiveGroups = inactiveGroups
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group statistics");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách hội nhóm đang hoạt động
        /// </summary>
        public async Task<List<GroupDTO>> GetActiveGroupsAsync(List<string>? allowedGroupIds = null)
        {
            try
            {
                IQueryable<Group> queryable = _groupRepository.AsQueryable()
                    .Where(g => g.IsActive);

                // allowedGroupIds filtering is deprecated - GIBA has full access
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    queryable = queryable.Where(g => allowedGroupIds.Contains(g.Id));
                }

                var groups = await queryable
                    .OrderBy(g => g.GroupName)
                    .ToListAsync();

                return groups.Select(g => new GroupDTO
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Rule = g.Rule,
                    IsActive = g.IsActive,
                    Logo = GetFullUrl(g.Logo),
                    CreatedDate = g.CreatedDate,
                    UpdatedDate = g.UpdatedDate,
                    MemberCount = 0,
                    MainActivities = g.MainActivities
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active groups");
                throw;
            }
        }

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho người ngoài (chưa tham gia) - chỉ hiển thị sự kiện công khai
        /// </summary>
        public async Task<object?> GetGroupPublicAsync(string id, string? userZaloId = null)
        {
            try
            {
                var group = await _groupRepository.AsQueryable()
                .Include(g => g.MembershipGroups)
                    .Include(g => g.Events.Where(e => e.IsActive && e.Type == 2)) // Chỉ lấy sự kiện công khai
                        .ThenInclude(e => e.EventSponsors)
                            .ThenInclude(es => es.Sponsor)
                    .Include(g => g.Events.Where(e => e.IsActive && e.Type == 2))
                        .ThenInclude(e => e.EventSponsors)
                            .ThenInclude(es => es.SponsorshipTier)
                    .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

                if (group == null)
                {
                    return null;
                }

                // Lấy số lượng thành viên (chỉ đếm membership chưa bị xóa)
                var memberCount = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.GroupId == id && mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true)
                    .CountAsync();

                // 1 = chưa đăng ký, 2 = chờ phê duyệt, 3 = đã tham gia, 4 = bị từ chối
                int joinStatus = 1; // Mặc định: chưa đăng ký
                var joinStatusText = "Chưa đăng ký";

                if (userZaloId != null)
                {
                    var membership = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.GroupId == id && mg.UserZaloId == userZaloId && mg.Membership != null && mg.Membership.IsDelete != true)
                        .FirstOrDefaultAsync();
                    if (membership != null)
                    {
                        if (membership.IsApproved == null)
                        {
                            joinStatus = 2; // Chờ phê duyệt
                            joinStatusText = "Chờ phê duyệt";
                        }
                        else if (membership.IsApproved == true)
                        {
                            joinStatus = 3; // Đã tham gia
                            joinStatusText = "Đã tham gia";
                        }
                        else if (membership.IsApproved == false)
                        {
                            joinStatus = 4; // Bị từ chối
                            joinStatusText = "Bị từ chối";
                        }
                    }
                }

                return new
                {
                    id = group.Id,
                    groupName = group.GroupName,
                    description = group.Description,
                    rule = group.Rule,
                    isActive = group.IsActive,
                    logo = GetFullUrl(group.Logo),
                    memberCount = memberCount,
                    createdDate = group.CreatedDate,
                    updatedDate = group.UpdatedDate,
                    joinStatus = joinStatus,
                    joinStatusText = joinStatusText,
                    mainActivities = group.MainActivities,
                    publicEvents = group.Events.Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        banner = GetFullUrl(e.Banner),
                        address = e.Address,
                        startTime = e.StartTime,
                        endTime = e.EndTime,
                        status = GetEventStatus(e.StartTime, e.EndTime),
                        statusText = GetEventStatusText(e.StartTime, e.EndTime)
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public group {GroupId}", id);
                throw;
            }
        }

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho thành viên - hiển thị đầy đủ sự kiện và danh sách thành viên
        /// </summary>
        public async Task<object?> GetGroupMemberAsync(string id, string userZaloId)
        {
            try
            {
                // Kiểm tra user đã tham gia nhóm chưa
                var membership = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .FirstOrDefaultAsync(mg => mg.GroupId == id && mg.UserZaloId == userZaloId && mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true);

                if (membership == null)
                {
                    return null; // User chưa tham gia nhóm
                }

                var group = await _groupRepository.AsQueryable()
                    .Include(g => g.Events.Where(e => e.IsActive)) // Lấy tất cả sự kiện
                        .ThenInclude(e => e.EventSponsors)
                            .ThenInclude(es => es.Sponsor)
                    .Include(g => g.Events.Where(e => e.IsActive))
                        .ThenInclude(e => e.EventSponsors)
                            .ThenInclude(es => es.SponsorshipTier)
                    .Include(g => g.Events.Where(e => e.IsActive))
                        .ThenInclude(e => e.EventRegistrations)
                    .Include(g => g.MembershipGroups.Where(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true)) // Chỉ lấy thành viên đã được duyệt và chưa bị xóa
                        .ThenInclude(mg => mg.Membership)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (group == null)
                {
                    return null;
                }

                return new
                {
                    id = group.Id,
                    groupName = group.GroupName,
                    description = group.Description,
                    rule = group.Rule,
                    isActive = group.IsActive,
                    logo = GetFullUrl(group.Logo),
                    memberCount = group.MembershipGroups.Count, // Đếm chỉ thành viên đã được duyệt
                    createdDate = group.CreatedDate,
                    updatedDate = group.UpdatedDate,
                    mainActivities = group.MainActivities,
                    members = group.MembershipGroups.Select(mg => new
                    {
                        membershipId = mg.Membership.Id,
                        userZaloId = mg.Membership.UserZaloId,
                        membershipName = mg.Membership.Fullname,
                        phoneNumber = mg.Membership.PhoneNumber,
                        avatar = mg.Membership.ZaloAvatar,
                        slug = mg.Membership.Slug,
                        joinedDate = mg.CreatedDate,
                        isApproved = mg.IsApproved, // Luôn là true vì đã filter ở trên
                        groupPosition = mg.GroupPosition,
                        sortOrder = mg.SortOrder
                    }).ToList(),
                    allEvents = group.Events.Select(e =>
                    {
                        var registration = e.EventRegistrations.FirstOrDefault(er => er.UserZaloId == userZaloId);
                        return new
                        {
                            id = e.Id,
                            title = e.Title,
                            banner = GetFullUrl(e.Banner),
                            address = e.Address,
                            startTime = e.StartTime,
                            endTime = e.EndTime,
                            status = GetEventStatus(e.StartTime, e.EndTime),
                            statusText = GetEventStatusText(e.StartTime, e.EndTime),
                            isRegister = registration != null && registration.Status != 3,
                            checkInStatus = registration != null ? (int)registration.CheckInStatus : 1, // 1 = NotCheckIn
                            checkInCode = registration?.CheckInCode,
                            checkInTime = registration?.CheckInTime,
                            registrationId = registration?.Id
                        };
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member group {GroupId} for user {UserZaloId}", id, userZaloId);
                throw;
            }
        }

        /// <summary>
        /// [MINI APP] Lấy chi tiết sự kiện - hiển thị đầy đủ thông tin sự kiện, quà, nhà tài trợ
        /// </summary>
        public async Task<object?> GetEventDetailAsync(string eventId, string? userZaloId = null)
        {
            try
            {
                var eventEntity = await _groupRepository.AsQueryable()
                    .SelectMany(g => g.Events)
                    .Where(e => e.Id == eventId && e.IsActive)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.Sponsor)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.SponsorshipTier)
                    .Include(e => e.EventGifts)
                    .Include(e => e.EventRegistrations.Where(er => er.UserZaloId == userZaloId))
                    .Include(e => e.Group)
                    .FirstOrDefaultAsync();

                if (eventEntity == null)
                {
                    return null;
                }

                // Kiểm tra quyền truy cập
                bool canViewEvent = false;
                if (eventEntity.Type == 2) // Công khai
                {
                    canViewEvent = true;
                }
                else if (eventEntity.Type == 1 && !string.IsNullOrEmpty(userZaloId)) // Nội bộ
                {
                    // Kiểm tra user có tham gia nhóm không
                    var membership = await _membershipGroupRepository.AsQueryable()
                        .Include(mg => mg.Membership)
                        .FirstOrDefaultAsync(mg => mg.GroupId == eventEntity.GroupId &&
                                                  mg.UserZaloId == userZaloId &&
                                                  mg.IsApproved == true &&
                                                  mg.Membership != null && mg.Membership.IsDelete != true);
                    canViewEvent = membership != null;
                }

                if (!canViewEvent)
                {
                    return null;
                }

                // Lấy thông tin đăng ký của user (nếu có)
                // Kiểm tra EventRegistration trước (đăng ký đơn lẻ)
                var userRegistration = eventEntity.EventRegistrations.FirstOrDefault(er => er.UserZaloId == userZaloId);

                // Nếu chưa có EventRegistration, kiểm tra GuestList (được mời trong đơn nhóm)
                GuestList? userGuestList = null;
                if (userRegistration == null && !string.IsNullOrEmpty(userZaloId))
                {
                    // Tìm user qua Membership để lấy phone/email
                    var userMembership = await _membershipRepository.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

                    if (userMembership != null)
                    {
                        // Tìm GuestList mà user được mời (match qua phone hoặc email)
                        userGuestList = await _guestListRepository.AsQueryable()
                            .Include(gl => gl.EventGuest)
                            .FirstOrDefaultAsync(gl =>
                                gl.EventGuest.EventId == eventId &&
                                (!string.IsNullOrEmpty(userMembership.PhoneNumber) && gl.GuestPhone == userMembership.PhoneNumber)
                            );
                    }
                }

                // Xác định thông tin đăng ký và check-in
                bool isRegister = false;
                int checkInStatus = 1; // 1 = NotCheckIn
                string? checkInCode = null;
                DateTime? checkInTime = null;
                string? registrationId = null;
                int? registrationStatus = null;
                string? registrationStatusText = null;

                if (userRegistration != null)
                {
                    // Có đăng ký đơn lẻ
                    isRegister = userRegistration.Status != 3; // Status 3 = Cancelled
                    checkInStatus = (int)userRegistration.CheckInStatus == 2 ? 2 : 1;
                    checkInCode = userRegistration.CheckInCode;
                    checkInTime = userRegistration.CheckInTime;
                    registrationId = userRegistration.Id;
                    registrationStatus = userRegistration.Status;
                    registrationStatusText = GetRegistrationStatusText(userRegistration.Status);
                }
                else if (userGuestList != null)
                {
                    // Có trong GuestList (được mời)
                    // Status: 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Cancelled, 4 = PendingRegistration, 5 = Registered
                    // IsRegister = true nếu status không phải Rejected (2) hoặc Cancelled (3)
                    isRegister = userGuestList.Status != (byte)EGuestStatus.Rejected && userGuestList.Status != (byte)EGuestStatus.Cancelled;
                    checkInStatus = userGuestList.CheckInStatus == true ? 2 : 1; // 2 = CheckedIn, 1 = NotCheckIn
                    checkInCode = !string.IsNullOrEmpty(userGuestList.CheckInCode) ? $"GUEST_{userGuestList.CheckInCode}" : null;
                    checkInTime = userGuestList.CheckInTime;
                    registrationId = userGuestList.Id;
                    registrationStatus = userGuestList.Status;
                    registrationStatusText = GetGuestStatusText(userGuestList.Status);
                }

                return new
                {
                    id = eventEntity.Id,
                    groupId = eventEntity.GroupId,
                    groupName = eventEntity.Group.GroupName,
                    title = eventEntity.Title,
                    content = eventEntity.Content,
                    banner = GetFullUrl(eventEntity.Banner),
                    images = !string.IsNullOrEmpty(eventEntity.Images)
                        ? eventEntity.Images.Split(',').Select(img => GetFullUrl(img)).ToList()
                        : new List<string>(),
                    startTime = eventEntity.StartTime,
                    endTime = eventEntity.EndTime,
                    type = eventEntity.Type,
                    typeText = eventEntity.Type == 1 ? "Nội bộ" : "Công khai",
                    status = GetEventStatus(eventEntity.StartTime, eventEntity.EndTime),
                    statusText = GetEventStatusText(eventEntity.StartTime, eventEntity.EndTime),
                    joinCount = eventEntity.JoinCount,
                    address = eventEntity.Address,
                    meetingLink = eventEntity.MeetingLink,
                    googleMapURL = eventEntity.GoogleMapURL,
                    isActive = eventEntity.IsActive,
                    createdDate = eventEntity.CreatedDate,
                    updatedDate = eventEntity.UpdatedDate,
                    // Thông tin đăng ký và check-in của user
                    isRegister = isRegister,
                    checkInStatus = checkInStatus,
                    checkInCode = checkInCode,
                    checkInTime = checkInTime,
                    registrationId = registrationId,
                    registrationStatus = registrationStatus,
                    registrationStatusText = registrationStatusText,
                    sponsorshipTiers = eventEntity.EventSponsors
                        .GroupBy(es => new { es.SponsorshipTierId, es.SponsorshipTier.TierName, es.SponsorshipTier.Image })
                        .Select(g => new
                        {
                            tierId = g.Key.SponsorshipTierId,
                            tierName = g.Key.TierName,
                            tierImage = GetFullUrl(g.Key.Image),
                            sponsors = g.Select(es => new
                            {
                                sponsorId = es.SponsorId,
                                sponsorName = es.Sponsor.SponsorName,
                                sponsorImage = GetFullUrl(es.Sponsor.Image),
                                sponsorIntroduction = es.Sponsor.Introduction
                            }).ToList()
                        }).ToList(),
                    gifts = eventEntity.EventGifts.Select(eg => new
                    {
                        id = eg.Id,
                        giftName = eg.GiftName,
                        images = !string.IsNullOrEmpty(eg.Images)
                            ? eg.Images.Split(',').Select(img => GetFullUrl(img)).ToList()
                            : new List<string>(),
                        quantity = eg.Quantity,
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event detail {EventId} for user {UserZaloId}", eventId, userZaloId);
                throw;
            }
        }

        /// <summary>
        /// [MINI APP] Lấy tất cả hội nhóm mà user đã tham gia (IsApproved = true)
        /// </summary>
        public async Task<PagedResult<GroupDTO>> GetAllGroupsForUserAsync(GroupQueryParameters query, string userZaloId, List<string> joinedGroupIds)
        {
            try
            {
                // Build query - chỉ lấy groups mà user đã tham gia
                IQueryable<Group> queryable = _groupRepository.AsQueryable()
                    .Where(g => g.IsActive && joinedGroupIds.Contains(g.Id))
                    .Include(g => g.MembershipGroups)
                        .ThenInclude(mg => mg.Membership);

                // GroupType and Type filtering is deprecated - GIBA has full access to all groups

                // Apply keyword filter
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(g =>
                        g.GroupName.Contains(query.Keyword) ||
                        (g.Description != null && g.Description.Contains(query.Keyword)));
                }

                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(g => g.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var groupDTOs = items.Select(g => new GroupDTO
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Rule = g.Rule,
                    IsActive = g.IsActive,
                    Logo = GetFullUrl(g.Logo),
                    CreatedDate = g.CreatedDate,
                    UpdatedDate = g.UpdatedDate,
                    MemberCount = g.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0,
                    IsJoined = true, // User đã tham gia tất cả groups này
                    JoinStatus = "approved",
                    JoinStatusText = "Đã tham gia",
                    MainActivities = g.MainActivities
                }).ToList();

                return new PagedResult<GroupDTO>
                {
                    Items = groupDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all groups for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        #region Private Helper Methods

        private string GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{relativePath}";
        }

        private byte GetEventStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (now < startTime)
                return 1; // Sắp diễn ra
            else if (now >= startTime && now <= endTime)
                return 2; // Đang diễn ra
            else
                return 3; // Đã kết thúc
        }

        private string GetEventStatusText(DateTime startTime, DateTime endTime)
        {
            var status = GetEventStatus(startTime, endTime);
            return status switch
            {
                1 => "Sắp diễn ra",
                2 => "Đang diễn ra",
                3 => "Đã kết thúc",
                _ => "Không xác định"
            };
        }

        private string GetRegistrationStatusText(int status)
        {
            return status switch
            {
                1 => "Chờ duyệt",
                2 => "Đã xác nhận",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetGuestStatusText(byte status)
        {
            return status switch
            {
                (byte)EGuestStatus.Pending => "Chờ xử lý",
                (byte)EGuestStatus.Approved => "Đã duyệt",
                (byte)EGuestStatus.Rejected => "Từ chối",
                (byte)EGuestStatus.Cancelled => "Đã hủy",
                (byte)EGuestStatus.PendingRegistration => "Chờ đăng ký",
                (byte)EGuestStatus.Registered => "Đã đăng ký",
                _ => "Không xác định"
            };
        }

        #endregion
    }
}
