using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Meetings;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Meetings;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Models.Request.Meetings;

namespace MiniAppGIBA.Services.Meetings
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<GroupPermission> _groupPermissionRepository;
        private readonly ILogger<MeetingService> _logger;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<Membership> _membershipRepository;

        public MeetingService(IUnitOfWork unitOfWork, ILogger<MeetingService> logger)
        {
            _unitOfWork = unitOfWork;
            _meetingRepository = unitOfWork.GetRepository<Meeting>();
            _rolesRepository = unitOfWork.GetRepository<Roles>();
            _groupRepository = unitOfWork.GetRepository<Group>();
            _groupPermissionRepository = unitOfWork.GetRepository<GroupPermission>();
            _membershipGroupRepository = unitOfWork.GetRepository<MembershipGroup>();
            _membershipRepository = unitOfWork.GetRepository<Membership>();
            _logger = logger;
        }

        public async Task<PagedResult<Meeting>> GetPage(MeetingQueryParams query, string roleId, string userId)
        {
            var queryable = _meetingRepository.AsQueryable();
            var role = await _rolesRepository.AsQueryable().FirstOrDefaultAsync(p => p.Id == roleId);
            var groupIds = new List<string>();

            // GIBA has full access - can see all meetings
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
                var user = await _membershipRepository.AsQueryable()
                    .FirstOrDefaultAsync(u => u.UserZaloId == userId);
                if (user != null)
                {
                    groupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == user.UserZaloId && mg.IsApproved == true)
                        .Select(mg => mg.GroupId)
                        .ToListAsync();
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

            if (query.MeetingType.HasValue)
            {
                queryable = queryable.Where(p => p.MeetingType == query.MeetingType.Value);
            }

            if (query.IsPublic.HasValue)
            {
                queryable = queryable.Where(p => p.IsPublic == query.IsPublic.Value);
            }

            // Filter by status - calculate dynamically based on StartDate and EndDate
            if (query.Status.HasValue)
            {
                var now = DateTime.Now;
                var statusValue = (byte)query.Status.Value;
                if (statusValue == 1) // Sắp diễn ra
                {
                    queryable = queryable.Where(p => p.StartDate > now);
                }
                else if (statusValue == 2) // Đang diễn ra
                {
                    queryable = queryable.Where(p => p.StartDate <= now && p.EndDate >= now);
                }
                else if (statusValue == 3) // Đã kết thúc
                {
                    queryable = queryable.Where(p => p.EndDate < now);
                }
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var keyword = query.Keyword.ToLower().Trim();
                queryable = queryable.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(keyword)) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                    (p.Location != null && p.Location.ToLower().Contains(keyword))
                );
            }

            if (query.Time != null)
            {
                var startOfDay = query.Time.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                queryable = queryable.Where(p => p.StartDate >= startOfDay && p.StartDate < endOfDay);
            }

            var totalItems = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

            var items = await queryable
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Meeting>()
            {
                Items = items,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<bool> CreateAsync(MeetingRequest request)
        {
            try
            {
                var role = await _rolesRepository.AsQueryable()
                    .FirstOrDefaultAsync(p => p.Name == request.RoleName);
                if (role == null)
                {
                    throw new Exception("Role not found");
                }

                // Auto-calculate status based on StartDate and EndDate
                var status = GetMeetingStatus(request.StartDate, request.EndDate);

                var meeting = new Meeting
                {
                    GroupId = request.GroupId,
                    GroupName = request.GroupName,
                    Title = request.Title,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MeetingType = request.MeetingType,
                    Location = request.Location,
                    MeetingLink = request.MeetingLink,
                    IsPublic = request.IsPublic,
                    Status = status, // Auto-calculated based on dates
                    CreatedBy = request.CreatedBy,
                    RoleId = role.Id
                };

                await _meetingRepository.AddAsync(meeting);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meeting with request: {@Request}", request);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string id, MeetingRequest request)
        {
            try
            {
                var meeting = await _meetingRepository.FindByIdAsync(id);
                if (meeting == null)
                {
                    throw new Exception("Meeting not found");
                }

                var role = await _rolesRepository.AsQueryable()
                    .FirstOrDefaultAsync(p => p.Name == request.RoleName);
                if (role == null)
                {
                    throw new Exception("Role not found");
                }

                // Auto-calculate status based on StartDate and EndDate
                var status = GetMeetingStatus(request.StartDate, request.EndDate);

                meeting.GroupId = request.GroupId;
                meeting.GroupName = request.GroupName;
                meeting.Title = request.Title;
                meeting.Description = request.Description;
                meeting.StartDate = request.StartDate;
                meeting.EndDate = request.EndDate;
                meeting.MeetingType = request.MeetingType;
                meeting.Location = request.Location;
                meeting.MeetingLink = request.MeetingLink;
                meeting.IsPublic = request.IsPublic;
                meeting.Status = status; // Auto-calculated based on dates
                meeting.RoleId = role.Id;
                meeting.UpdatedDate = DateTime.Now;

                _meetingRepository.Update(meeting);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meeting {Id} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var meeting = await _meetingRepository.FindByIdAsync(id);
                if (meeting == null)
                {
                    throw new Exception("Meeting not found");
                }

                _meetingRepository.Delete(meeting);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meeting {Id}", id);
                throw;
            }
        }

        public async Task<Meeting?> GetByIdAsync(string id)
        {
            return await _meetingRepository.FindByIdAsync(id);
        }

        private byte GetMeetingStatus(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;
            if (now < startDate)
                return 1; // Sắp diễn ra
            else if (now >= startDate && now <= endDate)
                return 2; // Đang diễn ra
            else
                return 3; // Đã kết thúc
        }

        private string GetStatusText(byte status)
        {
            return status switch
            {
                1 => "Sắp diễn ra",
                2 => "Đang diễn ra",
                3 => "Đã kết thúc",
                _ => "N/A"
            };
        }
    }
}

