using System.Drawing;
using AutoMapper;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Features.EventTrigger;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Payload;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Services.Groups
{
    public class MembershipGroupService : IMembershipGroupService
    {
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipGroupService> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public MembershipGroupService(
            IRepository<MembershipGroup> membershipGroupRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<MembershipGroupService> logger)
        {
            _membershipGroupRepository = membershipGroupRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<PagedResult<MembershipGroupDTO>> GetMembershipGroupsAsync(PendingApprovalQueryParameters query, List<string>? allowedGroupIds = null)
        {
            try
            {
                IQueryable<MembershipGroup> queryable = _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .Where(mg => mg.Membership.IsDelete != true);

                // Filter by allowed group IDs (for ADMIN users)
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    queryable = queryable.Where(mg => allowedGroupIds.Contains(mg.GroupId));
                }

                // Filter by approval status
                if (query.ShouldFilterByApprovalStatus)
                {
                    if (query.IsApproved.HasValue)
                    {
                        queryable = queryable.Where(mg => mg.IsApproved == query.IsApproved.Value);
                    }
                    else
                    {
                        // Default: only pending (null)
                        queryable = queryable.Where(mg => mg.IsApproved == null);
                    }
                }
                // If ShouldFilterByApprovalStatus = false, don't filter by approval status (get all)

                // Filter by group
                if (!string.IsNullOrEmpty(query.GroupId))
                {
                    queryable = queryable.Where(mg => mg.GroupId == query.GroupId);
                }

                // Filter by keyword (member name, phone, company)
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(mg =>
                        mg.Membership.Fullname.Contains(query.Keyword) ||
                        mg.Membership.PhoneNumber.Contains(query.Keyword) ||
                        (mg.Company != null && mg.Company.Contains(query.Keyword)));
                }

                // Sorting
                if (!string.IsNullOrEmpty(query.SortBy))
                {
                    queryable = query.SortDirection?.ToLower() == "desc"
                        ? queryable.OrderByDescending(mg => EF.Property<object>(mg, query.SortBy))
                        : queryable.OrderBy(mg => EF.Property<object>(mg, query.SortBy));
                }
                else
                {
                    // Default: newest first
                    queryable = queryable.OrderByDescending(mg => mg.CreatedDate);
                    Console.WriteLine("Sorting by CreatedDate");
                }


                var items = await queryable
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();
                var totalItems = items.Count();
                // Map to DTOs
                var dtos = items.Select(mg => new MembershipGroupDTO
                {
                    Id = mg.Id,
                    UserZaloId = mg.UserZaloId,
                    ZaloAvatar = mg.Membership?.ZaloAvatar,
                    GroupId = mg.GroupId,
                    GroupName = mg.Group?.GroupName ?? "",
                    MemberName = mg.Membership?.Fullname ?? "",
                    Reason = mg.Reason ?? "",
                    Company = mg.Company,
                    Position = mg.Position,
                    GroupPosition = mg.GroupPosition,
                    SortOrder = mg.SortOrder,
                    IsApproved = mg.IsApproved,
                    RejectReason = mg.RejectReason,
                    ApprovedDate = mg.ApprovedDate,
                    CreatedDate = mg.CreatedDate,
                    UpdatedDate = mg.UpdatedDate
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);
                return new PagedResult<MembershipGroupDTO>
                {
                    Items = dtos,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership groups with query: {@Query}", query);
                throw;
            }
        }

        public async Task<MembershipGroupDTO?> GetMembershipGroupByIdAsync(string id)
        {
            try
            {
                var membershipGroup = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .Where(mg => mg.Membership.IsDelete != true)
                    .FirstOrDefaultAsync(mg => mg.Id == id);

                if (membershipGroup == null)
                {
                    return null;
                }

                return new MembershipGroupDTO
                {
                    Id = membershipGroup.Id,
                    UserZaloId = membershipGroup.UserZaloId,
                    ZaloAvatar = membershipGroup.Membership?.ZaloAvatar,
                    GroupId = membershipGroup.GroupId,
                    GroupName = membershipGroup.Group?.GroupName ?? "",
                    MemberName = membershipGroup.Membership?.Fullname ?? "",
                    Reason = membershipGroup.Reason ?? "",
                    Company = membershipGroup.Company,
                    Position = membershipGroup.Position,
                    GroupPosition = membershipGroup.GroupPosition,
                    IsApproved = membershipGroup.IsApproved,
                    RejectReason = membershipGroup.RejectReason,
                    ApprovedDate = membershipGroup.ApprovedDate,
                    CreatedDate = membershipGroup.CreatedDate,
                    UpdatedDate = membershipGroup.UpdatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership group by id: {Id}", id);
                throw;
            }
        }
        public async Task<List<Dictionary<string, object>>> GetMembershipByGroupIdAsync(string groupId)
        {
            try
            {
                var membershipGroups = await _membershipGroupRepository.AsQueryable()
                  .Include(mg => mg.Membership)
                  .Include(mg => mg.Group)
                  .Where(mg => mg.GroupId == groupId)
                  .ToListAsync();
                return membershipGroups.Select(mg => new Dictionary<string, object>
                {
                    ["id"] = mg.Id,
                    ["userZaloId"] = mg.UserZaloId,
                    ["zaloAvatar"] = mg.Membership?.ZaloAvatar ?? "",
                    ["groupId"] = mg.GroupId,
                    ["groupName"] = mg.Group?.GroupName ?? "",
                    ["memberName"] = mg.Membership?.Fullname ?? "",
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership by group id: {GroupId}", groupId);
                throw;
            }
        }
        public async Task<List<MembershipGroupDTO>> GetMembershipGroupByGroupIdAsync(string groupId)
        {
            try
            {
                // Only get approved members (isApproved == true) for speaker selection
                // Cho phép ApprovalStatus == 1 (Approved) hoặc == 4 (PendingRegistration)
                var membershipGroups = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .Where(mg => mg.GroupId == groupId && mg.IsApproved == true && mg.Membership.IsDelete != true)
                    .OrderBy(mg => mg.Membership.Fullname)
                    .ToListAsync();

                if (membershipGroups == null || !membershipGroups.Any())
                {
                    return new List<MembershipGroupDTO>();
                }

                return membershipGroups.Select(mg => new MembershipGroupDTO
                {
                    Id = mg.Id,
                    UserZaloId = mg.UserZaloId,
                    ZaloAvatar = mg.Membership?.ZaloAvatar,
                    GroupId = mg.GroupId,
                    GroupName = mg.Group?.GroupName ?? "",
                    MemberName = mg.Membership?.Fullname ?? "",
                    Reason = mg.Reason ?? "",
                    Company = mg.Company,
                    Position = mg.Position,
                    GroupPosition = mg.GroupPosition,
                    IsApproved = mg.IsApproved,
                    RejectReason = mg.RejectReason,
                    ApprovedDate = mg.ApprovedDate,
                    CreatedDate = mg.CreatedDate,
                    UpdatedDate = mg.UpdatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership group by group id: {GroupId}", groupId);
                throw;
            }
        }

        public async Task<MembershipGroupDTO> ApproveOrRejectAsync(ApproveRejectRequest request)
        {
            try
            {
                var membershipGroup = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .FirstOrDefaultAsync(mg => mg.Id == request.Id);

                if (membershipGroup == null)
                {
                    throw new CustomException(404, "Không tìm thấy đơn xin tham gia!");
                }

                // Check if already approved/rejected
                if (membershipGroup.IsApproved != null)
                {
                    throw new CustomException(400, $"Đơn này đã được {(membershipGroup.IsApproved.Value ? "phê duyệt" : "từ chối")} trước đó!");
                }

                // Validate reject reason
                if (!request.IsApproved && string.IsNullOrWhiteSpace(request.RejectReason))
                {
                    throw new CustomException(400, "Vui lòng nhập lý do từ chối!");
                }


                try
                {
                    // Use direct SQL update to bypass EF tracking issues
                    var updateResult = await _unitOfWork.GetRepository<MembershipGroup>()
                        .AsQueryable()
                        .Where(mg => mg.Id == request.Id)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(mg => mg.IsApproved, request.IsApproved)
                            .SetProperty(mg => mg.RejectReason, request.IsApproved ? null : request.RejectReason)
                            .SetProperty(mg => mg.ApprovedDate, DateTime.Now)
                            .SetProperty(mg => mg.UpdatedDate, DateTime.Now));


                    if (updateResult == 0)
                    {
                        throw new CustomException(400, "Không thể cập nhật trạng thái phê duyệt!");
                    }
                    else
                    {
                        var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                        var payload = CreateGroupPayloadFromData(membershipGroup);
                        BackgroundJob.Schedule<MembershipGroupService>(
                            x => x.TriggerGroupEvent("Membership.UpdateGroupStatus", payload),
                            TimeSpan.FromSeconds(delayInSeconds)
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating membership group {Id}", request.Id);
                    throw;
                }

                // Update the local object for return
                membershipGroup.IsApproved = request.IsApproved;
                membershipGroup.RejectReason = request.IsApproved ? null : request.RejectReason;
                membershipGroup.ApprovedDate = DateTime.Now;
                membershipGroup.UpdatedDate = DateTime.Now;

                return new MembershipGroupDTO
                {
                    Id = membershipGroup.Id,
                    UserZaloId = membershipGroup.UserZaloId,
                    ZaloAvatar = membershipGroup.Membership?.ZaloAvatar,
                    GroupId = membershipGroup.GroupId,
                    GroupName = membershipGroup.Group?.GroupName ?? "",
                    MemberName = membershipGroup.Membership?.Fullname ?? "",
                    Reason = membershipGroup.Reason ?? "",
                    Company = membershipGroup.Company,
                    Position = membershipGroup.Position,
                    GroupPosition = membershipGroup.GroupPosition,
                    IsApproved = membershipGroup.IsApproved,
                    RejectReason = membershipGroup.RejectReason,
                    ApprovedDate = membershipGroup.ApprovedDate,
                    CreatedDate = membershipGroup.CreatedDate,
                    UpdatedDate = membershipGroup.UpdatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving/rejecting membership group {Id}", request.Id);
                throw;
            }
        }

        public async Task<int> GetPendingCountAsync(List<string>? allowedGroupIds = null)
        {
            try
            {
                var query = _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.IsApproved == null);

                // Filter by allowed group IDs (for ADMIN users)
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    query = query.Where(mg => allowedGroupIds.Contains(mg.GroupId));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending count");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách MembershipGroup của user theo UserZaloId
        /// </summary>
        public async Task<List<MembershipGroupDTO>> GetMembershipGroupsByUserZaloIdAsync(string userZaloId)
        {
            try
            {
                var membershipGroups = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .Where(mg => mg.UserZaloId == userZaloId && mg.Membership.IsDelete != true)
                    .ToListAsync();

                return membershipGroups.Select(mg => new MembershipGroupDTO
                {
                    Id = mg.Id,
                    UserZaloId = mg.UserZaloId,
                    ZaloAvatar = mg.Membership?.ZaloAvatar,
                    GroupId = mg.GroupId,
                    MemberName = mg.Membership?.Fullname ?? "N/A",
                    GroupName = mg.Group?.GroupName ?? "N/A",
                    Company = mg.Company,
                    Position = mg.Position,
                    GroupPosition = mg.GroupPosition,
                    Reason = mg.Reason ?? "",
                    IsApproved = mg.IsApproved,
                    RejectReason = mg.RejectReason,
                    ApprovedDate = mg.ApprovedDate,
                    CreatedDate = mg.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership groups for UserZaloId: {UserZaloId}", userZaloId);
                throw;
            }
        }

        #region Private Methods

        public async Task TriggerGroupEvent(string eventName, GroupPayload payload)
        {
            await _mediator.Send(new EmitEventArgs
            {
                EventName = eventName,
                TriggerPhoneNumber = string.IsNullOrEmpty(payload.PhoneNumber) ? null : payload.PhoneNumber,
                TriggerZaloIdByOA = string.IsNullOrEmpty(payload.UserZaloIdByOA) ? null : payload.UserZaloIdByOA,
                Payload = payload
            });
        }

        public GroupPayload CreateGroupPayloadFromData(MembershipGroup membershipGroup)
        {
            var membership = membershipGroup.Membership;
            var group = membershipGroup.Group;

            return new GroupPayload
            {
                UserZaloId = membership.UserZaloId,
                UserZaloName = membership.UserZaloName,
                UserZaloIdByOA = membership.UserZaloIdByOA,
                PhoneNumber = membership.PhoneNumber,
                Reason = membershipGroup.Reason,
                Company = membershipGroup.Company,
                Position = membershipGroup.Position,
                IsApproved = membershipGroup.IsApproved,
                RejectReason = membershipGroup.RejectReason,
                ApprovedDate = membershipGroup.ApprovedDate,
                GroupName = group.GroupName
            };
        }

        /// <summary>
        /// Thêm thành viên vào nhóm với trạng thái đã duyệt
        /// </summary>
        public async Task<MembershipGroupDTO> AddMemberToGroupAsync(string groupId, string userZaloId)
        {
            try
            {
                // Check if member already exists in group
                var existingMembership = await _membershipGroupRepository.AsQueryable()
                    .FirstOrDefaultAsync(mg => mg.GroupId == groupId && mg.UserZaloId == userZaloId);

                if (existingMembership != null)
                {
                    if (existingMembership.IsApproved == true)
                    {
                        throw new CustomException(400, "Thành viên đã tồn tại trong nhóm");
                    }
                    else if (existingMembership.IsApproved == null)
                    {
                        throw new CustomException(400, "Thành viên đang chờ duyệt tham gia nhóm");
                    }
                    else
                    {
                        // If rejected before, delete and create new
                        _membershipGroupRepository.Delete(existingMembership);
                    }
                }

                // Create new membership group with approved status
                var membershipGroup = new MembershipGroup
                {
                    UserZaloId = userZaloId,
                    GroupId = groupId,
                    IsApproved = true,
                    ApprovedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _membershipGroupRepository.AddAsync(membershipGroup);
                await _unitOfWork.SaveChangesAsync();

                // Load navigation properties
                var result = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .FirstOrDefaultAsync(mg => mg.Id == membershipGroup.Id);

                if (result == null)
                {
                    throw new Exception("Không thể tải thông tin thành viên sau khi thêm");
                }

                return new MembershipGroupDTO
                {
                    Id = result.Id,
                    UserZaloId = result.UserZaloId,
                    ZaloAvatar = result.Membership?.ZaloAvatar,
                    GroupId = result.GroupId,
                    GroupName = result.Group?.GroupName ?? "",
                    MemberName = result.Membership?.Fullname ?? "",
                    Reason = result.Reason ?? "",
                    Company = result.Company,
                    Position = result.Position,
                    GroupPosition = result.GroupPosition,
                    IsApproved = result.IsApproved,
                    RejectReason = result.RejectReason,
                    ApprovedDate = result.ApprovedDate,
                    CreatedDate = result.CreatedDate,
                    UpdatedDate = result.UpdatedDate
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member {UserZaloId} to group {GroupId}", userZaloId, groupId);
                throw;
            }
        }

        #endregion
    }
}

