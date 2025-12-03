using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.API;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;
using MiniAppGIBA.Models.Response.Common;
using MiniAppGIBA.Service.Groups;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Services.Groups;
using MiniAppGIBA.Services.CustomFields;
using MiniAppGIBA.Enum;
namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : BaseAPIController
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupsController> _logger;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUrl _url;
        private readonly IMembershipGroupService _membershipGroupService;
        private readonly ICustomFieldFormHandler _customFieldFormHandler;
        private readonly ICustomFieldService _customFieldService;

        public GroupsController(
            IGroupService groupService,
            ILogger<GroupsController> logger,
            IRepository<MembershipGroup> membershipGroupRepository,
            IRepository<Membership> membershipRepository,
            IUnitOfWork unitOfWork,
            IMembershipGroupService membershipGroupService,
            IUrl url,
            ICustomFieldFormHandler customFieldFormHandler,
            ICustomFieldService customFieldService)
        {
            _groupService = groupService;
            _logger = logger;
            _membershipGroupRepository = membershipGroupRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _url = url;
            _membershipGroupService = membershipGroupService;
            _customFieldFormHandler = customFieldFormHandler;
            _customFieldService = customFieldService;
        }

        /// <summary>
        /// Lấy danh sách hội nhóm với phân trang và filter
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] GroupQueryParameters query)
        {
            try
            {
                // Lấy userZaloId từ JWT token (nếu có)
                var userZaloId = User.FindFirst("UserZaloId")?.Value;

                var result = await _groupService.GetGroupsAsync(query, userZaloId);

                return Ok(new ApiResponse<PagedResult<GroupDTO>>
                {
                    Code = 0,
                    Message = "Lấy danh sách hội nhóm thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups");
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách hội nhóm"
                });
            }
        }

        /// Lấy tất cả hội nhóm với phân trang - hiển thị tất cả groups, isJoined dựa trên việc user đã tham gia hay chưa
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroups([FromQuery] GroupQueryParameters query)
        {
            try
            {
                // Lấy userZaloId từ JWT token (nếu có)
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    Console.WriteLine("123123");
                    if (query.IsActive != null)
                    {
                        var resultActive = new PagedResult<GroupDTO>
                        {
                            Items = new List<GroupDTO>(),
                            TotalItems = 0,
                            Page = query.Page,
                            PageSize = query.PageSize,
                            TotalPages = 0
                        };
                        return Ok(resultActive);
                    }
                    var result = await _groupService.GuestGetPage(query.Type, query.IsActive, query.Page, query.PageSize);
                    return Ok(new ApiResponse<PagedResult<GroupDTO>>
                    {
                        Code = 0,
                        Message = "Lấy danh sách tất cả hội nhóm thành công",
                        Data = result
                    });
                }
                else
                {
                    Console.WriteLine("dbasjkdadasdasbdjkas");
                    var result = await _groupService.GetAllGroupsAsync(query, userZaloId);
                    return Ok(new ApiResponse<PagedResult<GroupDTO>>
                    {
                        Code = 0,
                        Message = "Lấy danh sách tất cả hội nhóm thành công",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all groups");
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách tất cả hội nhóm"
                });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy tất cả hội nhóm mà user đã tham gia (IsApproved = true)
        /// Response format giống với /all
        /// </summary>
        [HttpGet("all-for-user")]
        [Authorize]
        public async Task<IActionResult> GetAllForUser([FromQuery] GroupQueryParameters query)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Lấy danh sách groupId mà user đã tham gia (IsApproved = true)
                var joinedGroupIds = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .Select(mg => mg.GroupId)
                    .ToListAsync();

                if (!joinedGroupIds.Any())
                {
                    return Ok(new ApiResponse<PagedResult<GroupDTO>>
                    {
                        Code = 0,
                        Message = "Lấy danh sách hội nhóm thành công",
                        Data = new PagedResult<GroupDTO>
                        {
                            Items = new List<GroupDTO>(),
                            TotalItems = 0,
                            Page = query.Page,
                            PageSize = query.PageSize,
                            TotalPages = 0
                        }
                    });
                }

                // Lấy tất cả groups mà user đã tham gia với filter
                var result = await _groupService.GetAllGroupsForUserAsync(query, userZaloId, joinedGroupIds);

                return Ok(new ApiResponse<PagedResult<GroupDTO>>
                {
                    Code = 0,
                    Message = "Lấy danh sách hội nhóm thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all groups for user {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách hội nhóm"
                });
            }
        }

        // [HttpGet("guest-get")]
        // public async Task<IActionResult> GuestGetGroups([FromQuery] string? type, string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10){
        //     try
        //     {
        //         var result = await _groupService.GuestGetPage(type, status, page, pageSize);
        //         return Ok(new ApiResponse<PagedResult<GroupDTO>>
        //         {
        //             Code = 0,
        //             Message = "Lấy danh sách tất cả hội nhóm thành công",
        //             Data = result
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting all groups");
        //         return StatusCode(500, new ApiResponse<object>
        //         {
        //             Code = 1,
        //             Message = "Có lỗi xảy ra khi lấy danh sách tất cả hội nhóm"
        //         });
        //     }
        // }

        /// Lấy thông tin chi tiết hội nhóm
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            try
            {
                var groupDTO = await _groupService.GetGroupByIdAsync(id);
                if (groupDTO == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy hội nhóm"
                    });
                }

                return Ok(new ApiResponse<GroupDetailDTO>
                {
                    Code = 0,
                    Message = "Lấy thông tin hội nhóm thành công",
                    Data = groupDTO
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group {GroupId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy thông tin hội nhóm"
                });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho người ngoài (chưa tham gia) - chỉ hiển thị sự kiện công khai
        /// </summary>
        [HttpGet("{id}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupPublic(string id)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                var groupDTO = await _groupService.GetGroupPublicAsync(id, userZaloId);
                if (groupDTO == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy hội nhóm"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy thông tin hội nhóm thành công",
                    Data = groupDTO
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public group {GroupId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy thông tin hội nhóm"
                });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho thành viên - hiển thị đầy đủ sự kiện và danh sách thành viên
        /// </summary>
        [HttpGet("{id}/member")]
        public async Task<IActionResult> GetGroupMember(string id)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var groupDTO = await _groupService.GetGroupMemberAsync(id, userZaloId);
                if (groupDTO == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy hội nhóm hoặc bạn chưa tham gia hội nhóm này"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy thông tin hội nhóm thành công",
                    Data = groupDTO
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member group {GroupId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy thông tin hội nhóm"
                });
            }
        }
        [HttpGet("membership-in-group/{groupId}")]
        public async Task<IActionResult> GetMembershipInGroup(string groupId)
        {
            try
            {
                var memberships = await _membershipGroupService.GetMembershipByGroupIdAsync(groupId);
                return Ok(new ApiResponse<List<Dictionary<string, object>>>
                {
                    Code = 0,
                    Message = "Lấy danh sách thành viên trong hội nhóm thành công",
                    Data = memberships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership in group {GroupId}", groupId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách thành viên trong hội nhóm"
                });
            }
        }
        /// <summary>
        /// [MINI APP] Lấy chi tiết sự kiện - hiển thị đầy đủ thông tin sự kiện, quà, nhà tài trợ
        /// </summary>
        [HttpGet("events/{eventId}")]
        public async Task<IActionResult> GetEventDetail(string eventId)
        {
            try
            {
                // Lấy userZaloId từ JWT token (nếu có)
                var userZaloId = User.FindFirst("UserZaloId")?.Value;

                var eventDetail = await _groupService.GetEventDetailAsync(eventId, userZaloId);
                if (eventDetail == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy sự kiện hoặc bạn không có quyền xem sự kiện này"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy chi tiết sự kiện thành công",
                    Data = eventDetail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event detail {EventId}", eventId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy chi tiết sự kiện"
                });
            }
        }

        /// <summary>
        /// Tạo hội nhóm mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromForm] CreateGroupRequest request)
        {
            try
            {
                var groupDTO = await _groupService.CreateGroupAsync(request);

                return CreatedAtAction(nameof(GetGroup), new { id = groupDTO.Id }, new ApiResponse<GroupDTO>
                {
                    Code = 0,
                    Message = "Tạo hội nhóm thành công",
                    Data = groupDTO
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = 1,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi tạo hội nhóm"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin hội nhóm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromForm] UpdateGroupRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState
                    });
                }

                var groupDTO = await _groupService.UpdateGroupAsync(id, request);

                return Ok(new ApiResponse<GroupDTO>
                {
                    Code = 0,
                    Message = "Cập nhật hội nhóm thành công",
                    Data = groupDTO
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Không tìm thấy hội nhóm"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = 1,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi cập nhật hội nhóm"
                });
            }
        }

        /// <summary>
        /// Xóa hội nhóm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            try
            {
                var result = await _groupService.DeleteGroupAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy hội nhóm"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Xóa hội nhóm thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = 1,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi xóa hội nhóm"
                });
            }
        }

        /// <summary>
        /// Thay đổi trạng thái hội nhóm
        /// </summary>
        // [HttpPatch("{id}/status")]
        // public async Task<IActionResult> ToggleGroupStatus(string id)
        // {
        //     try
        //     {
        //         var result = await _groupService.ToggleGroupStatusAsync(id);
        //         if (!result)
        //         {
        //             return NotFound(new ApiResponse<object>
        //             {
        //                 Code = 1,
        //                 Message = "Không tìm thấy hội nhóm"
        //             });
        //         }

        //         return Ok(new ApiResponse<object>
        //         {
        //             Code = 0,
        //             Message = "Thay đổi trạng thái hội nhóm thành công"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error toggling group status {GroupId}", id);
        //         return StatusCode(500, new ApiResponse<object>
        //         {
        //             Code = 1,
        //             Message = "Có lỗi xảy ra khi thay đổi trạng thái hội nhóm"
        //         });
        //     }
        // }

        #region Mini App APIs


        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> JoinGroup(string groupId, [FromBody] JoinGroupRequest request)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var group = await _groupService.GetGroupByIdAsync(groupId);
                if (group == null || !group.IsActive)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Hội nhóm không tồn tại hoặc đã ngừng hoạt động"
                    });
                }

                var existingMembership = await _membershipGroupRepository.GetFirstOrDefaultAsync(
                    mg => mg.UserZaloId == userZaloId && mg.GroupId == groupId);

                if (existingMembership != null)
                {
                    if (existingMembership.IsApproved == true)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Code = 1,
                            Message = "Bạn đã là thành viên của hội nhóm này"
                        });
                    }
                    else if (existingMembership.IsApproved == null)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Code = 1,
                            Message = "Bạn đã gửi yêu cầu tham gia hội nhóm này và đang chờ xét duyệt"
                        });
                    }
                }

                if (existingMembership != null && existingMembership.IsApproved == false)
                {
                    _membershipGroupRepository.Delete(existingMembership);
                }

                var membershipGroup = new MembershipGroup
                {
                    UserZaloId = userZaloId,
                    GroupId = groupId,
                    Reason = request.Reason,
                    Company = request.Company,
                    Position = request.Position,
                    IsApproved = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _membershipGroupRepository.AddAsync(membershipGroup);
                await _unitOfWork.SaveChangesAsync();

                // Handle custom field submission if provided
                if (request.CustomFieldValues != null && request.CustomFieldValues.Any())
                {
                    try
                    {
                        // Validate custom field values
                        var validationResult = await _customFieldFormHandler.ValidateFormAsync(
                            ECustomFieldEntityType.GroupMembership,
                            groupId,
                            request.CustomFieldValues);

                        if (!validationResult.IsValid)
                        {
                            var errorMessages = string.Join("; ", validationResult.Errors.Values);
                            return BadRequest(new ApiResponse<object>
                            {
                                Code = 1,
                                Message = $"Lỗi xác thực form: {errorMessages}",
                                Data = validationResult.Errors
                            });
                        }

                        // Submit custom field values
                        await _customFieldFormHandler.SubmitFormAsync(
                            ECustomFieldEntityType.GroupMembership,
                            membershipGroup.Id,
                            request.CustomFieldValues);

                        // Mark that custom fields have been submitted
                        membershipGroup.HasCustomFieldsSubmitted = true;
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Custom field values submitted for membership group {MembershipGroupId}", membershipGroup.Id);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogWarning(ex, "Custom field validation failed for membership group {MembershipGroupId}", membershipGroup.Id);
                        return BadRequest(new ApiResponse<object>
                        {
                            Code = 1,
                            Message = $"Lỗi xác thực form: {ex.Message}"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error submitting custom fields for membership group {MembershipGroupId}", membershipGroup.Id);
                        return Error("Có lỗi xảy ra khi lưu thông tin form", 500);
                    }
                }

                return Success(new
                {
                    message = "Gửi yêu cầu tham gia hội nhóm thành công! Vui lòng chờ admin xét duyệt.",
                    groupName = group.GroupName,
                    status = "pending",
                    membershipGroupId = membershipGroup.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group {GroupId} for user {UserZaloId}", groupId, User.FindFirst("UserZaloId")?.Value);
                return Error("Có lỗi xảy ra khi tham gia hội nhóm", 500);
            }
        }


        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Sửa lỗi logic: phải có dấu ngoặc để chỉ lấy groups của user hiện tại
                var userMemberships = await _membershipGroupRepository.AsQueryable()
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.Group)
                    .Where(mg => mg.UserZaloId == userZaloId && (mg.IsApproved == null || mg.IsApproved == true) && mg.Membership.IsDelete != true)
                    .OrderByDescending(mg => mg.CreatedDate)
                    .ToListAsync();

                if (!userMemberships.Any())
                {
                    return Ok(new ApiResponse<object>
                    {
                        Code = 0,
                        Message = "Lấy danh sách hội nhóm thành công",
                        Data = new List<object>()
                    });
                }
                var groupIds = userMemberships.Select(mg => mg.GroupId).ToList();
                var groups = await _unitOfWork.GetRepository<Group>()
                    .AsQueryable()
                    .Where(g => groupIds.Contains(g.Id) && g.IsActive)
                    .Include(g => g.MembershipGroups)
                        .ThenInclude(mg => mg.Membership)
                    .ToListAsync();

                var result = groups.Select(g =>
                {
                    var userMembership = userMemberships.FirstOrDefault(mg => mg.GroupId == g.Id);
                    var memberCount = g.MembershipGroups?.Count(mg => mg.IsApproved == true && mg.Membership != null && mg.Membership.IsDelete != true) ?? 0;

                    string joinStatus = "unknown";
                    bool isJoined = false;

                    if (userMembership != null)
                    {
                        switch (userMembership.IsApproved)
                        {
                            case true:
                                joinStatus = "approved";
                                isJoined = true;
                                break;
                            case false:
                                joinStatus = "rejected";
                                isJoined = false;
                                break;
                            case null:
                                joinStatus = "pending";
                                isJoined = false;
                                break;
                        }
                    }

                    return new
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description,
                        Rule = g.Rule,
                        IsActive = g.IsActive,
                        MemberCount = memberCount,
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate,
                        Logo = GetFullUrl(g.Logo),
                        IsJoined = isJoined,
                        JoinStatus = joinStatus,
                        JoinStatusText = joinStatus switch
                        {
                            "approved" => "Đã tham gia",
                            "pending" => "Chờ phê duyệt",
                            "rejected" => "Bị từ chối",
                            _ => "Không xác định"
                        },
                        JoinRequestDate = userMembership?.CreatedDate,
                        ApprovedDate = userMembership?.ApprovedDate,
                        RejectReason = userMembership?.RejectReason
                    };
                }).ToList();

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy danh sách hội nhóm thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups for {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách hội nhóm"
                });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy danh sách đơn xin tham gia của user (bao gồm cả pending, approved, rejected)
        /// </summary>
        [HttpGet("my-join-requests")]
        public async Task<IActionResult> GetMyJoinRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? status = null) // null = tất cả, "true" = approved, "false" = rejected
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Build base query
                var query = _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.UserZaloId == userZaloId);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    if (bool.TryParse(status, out bool statusValue))
                    {
                        query = query.Where(mg => mg.IsApproved == statusValue);
                    }
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(mg =>
                        (mg.Reason != null && mg.Reason.Contains(keyword)) ||
                        (mg.Company != null && mg.Company.Contains(keyword)) ||
                        (mg.Position != null && mg.Position.Contains(keyword)));
                }

                // Get total count for pagination
                var totalItems = await query.CountAsync();

                // Apply pagination and include
                var joinRequests = await query
                    .Include(mg => mg.Group)
                    .OrderByDescending(mg => mg.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = joinRequests.Select(mg => new
                {
                    id = mg.Id,
                    groupId = mg.GroupId,
                    groupName = mg.Group?.GroupName ?? "",
                    reason = mg.Reason,
                    company = mg.Company,
                    position = mg.Position,
                    isApproved = mg.IsApproved,
                    statusText = mg.IsApproved switch
                    {
                        null => "Chờ xét duyệt",
                        true => "Đã duyệt",
                        false => "Từ chối"
                    },
                    rejectReason = mg.RejectReason,
                    approvedDate = mg.ApprovedDate,
                    createdDate = mg.CreatedDate,
                    updatedDate = mg.UpdatedDate,
                    canEdit = mg.IsApproved == null // Chỉ cho phép sửa nếu đang chờ duyệt
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                return Success(new
                {
                    items = result,
                    totalItems = totalItems,
                    totalPages = totalPages,
                    currentPage = page,
                    pageSize = pageSize
                }, "Lấy danh sách đơn xin tham gia thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting join requests for user {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return Error("Có lỗi xảy ra khi lấy danh sách đơn xin tham gia", 500);
            }
        }

        /// <summary>
        /// [MINI APP] Lấy chi tiết đơn xin tham gia
        /// </summary>
        [HttpGet("my-join-requests/{id}")]
        public async Task<IActionResult> GetJoinRequestDetail(string id)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var joinRequest = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.Id == id && mg.UserZaloId == userZaloId)
                    .Include(mg => mg.Group)
                    .FirstOrDefaultAsync();

                if (joinRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy đơn xin tham gia"
                    });
                }

                var result = new
                {
                    id = joinRequest.Id,
                    groupId = joinRequest.GroupId,
                    groupName = joinRequest.Group?.GroupName ?? "",
                    reason = joinRequest.Reason,
                    company = joinRequest.Company,
                    position = joinRequest.Position,
                    isApproved = joinRequest.IsApproved,
                    statusText = joinRequest.IsApproved switch
                    {
                        null => "Chờ xét duyệt",
                        true => "Đã duyệt",
                        false => "Từ chối"
                    },
                    rejectReason = joinRequest.RejectReason,
                    approvedDate = joinRequest.ApprovedDate,
                    createdDate = joinRequest.CreatedDate,
                    updatedDate = joinRequest.UpdatedDate,
                    canEdit = joinRequest.IsApproved == null
                };

                return Success(result, "Lấy thông tin đơn xin tham gia thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting join request {Id}", id);
                return Error("Có lỗi xảy ra khi lấy thông tin đơn xin tham gia", 500);
            }
        }

        /// <summary>
        /// [MINI APP] Lấy chi tiết nhóm đã tham gia kèm custom field values mà user đã nhập khi đăng ký
        /// </summary>
        [HttpGet("my-membership/{groupId}")]
        public async Task<IActionResult> GetMyMembershipDetail(string groupId)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }
                Console.WriteLine("ádsad" + groupId + userZaloId);
                // Lấy thông tin membership group kèm custom field values
                var membershipGroup = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.GroupId == groupId && mg.UserZaloId == userZaloId)
                    .Include(mg => mg.Group)
                    .Include(mg => mg.Membership)
                    .Include(mg => mg.CustomFieldValues)
                        .ThenInclude(cfv => cfv.CustomField)
                    .FirstOrDefaultAsync();

                if (membershipGroup == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Bạn chưa đăng ký tham gia nhóm này"
                    });
                }

                // Lấy thông tin group chi tiết
                var group = membershipGroup.Group;
                if (group == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin nhóm"
                    });
                }

                // Lấy số lượng thành viên đã duyệt
                var memberCount = await _membershipGroupRepository.AsQueryable()
                    .CountAsync(mg => mg.GroupId == groupId && mg.IsApproved == true);

                // Lấy tất cả custom fields của group để hiển thị đầy đủ (kể cả chưa có giá trị)
                var allCustomFields = await _customFieldService.GetFieldsByEntityAsync(ECustomFieldEntityType.GroupMembership, groupId);

                // Map custom field values đã nhập
                var customFieldValuesDict = membershipGroup.CustomFieldValues
                    .Where(cfv => cfv.CustomField != null)
                    .ToDictionary(cfv => cfv.CustomFieldId, cfv => cfv);

                // Tạo danh sách custom fields với giá trị
                var customFieldsWithValues = allCustomFields.Select(cf =>
                {
                    customFieldValuesDict.TryGetValue(cf.Id, out var fieldValue);
                    return new
                    {
                        fieldId = cf.Id,
                        fieldName = cf.FieldName,
                        fieldType = cf.FieldType.ToString(),
                        fieldTypeText = cf.FieldTypeText,
                        isRequired = cf.IsRequired,
                        options = cf.FieldOptions,
                        tabId = cf.CustomFieldTabId,
                        displayOrder = cf.DisplayOrder,
                        value = fieldValue?.FieldValue,
                        hasValue = fieldValue != null
                    };
                }).OrderBy(cf => cf.displayOrder).ToList();

                // Group custom fields by tab
                var customFieldsByTab = customFieldsWithValues
                    .GroupBy(cf => cf.tabId)
                    .Select(g => new
                    {
                        tabId = g.Key,
                        tabName = "Thông tin chung",
                        fields = g.ToList()
                    })
                    .ToList();

                var result = new
                {
                    // Thông tin membership
                    membershipGroupId = membershipGroup.Id,
                    reason = membershipGroup.Reason,
                    company = membershipGroup.Company,
                    position = membershipGroup.Position,
                    groupPosition = membershipGroup.GroupPosition,
                    isApproved = membershipGroup.IsApproved,
                    statusText = membershipGroup.IsApproved switch
                    {
                        null => "Chờ xét duyệt",
                        true => "Đã duyệt",
                        false => "Từ chối"
                    },
                    rejectReason = membershipGroup.RejectReason,
                    approvedDate = membershipGroup.ApprovedDate,
                    joinRequestDate = membershipGroup.CreatedDate,
                    hasCustomFieldsSubmitted = membershipGroup.HasCustomFieldsSubmitted,
                    canEdit = membershipGroup.IsApproved == null,

                    // Thông tin group
                    group = new
                    {
                        id = group.Id,
                        groupName = group.GroupName,
                        description = group.Description,
                        rule = group.Rule,
                        logo = GetFullUrl(group.Logo),
                        isActive = group.IsActive,
                        memberCount = memberCount,
                        createdDate = group.CreatedDate
                    },

                    // Custom fields với giá trị đã nhập
                    // customFields = customFieldsWithValues,
                    customFieldsByTab = customFieldsByTab
                };

                return Success(result, "Lấy thông tin chi tiết thành viên trong nhóm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership detail for group {GroupId}", groupId);
                return Error("Có lỗi xảy ra khi lấy thông tin chi tiết", 500);
            }
        }

        /// <summary>
        /// [MINI APP] Chỉnh sửa đơn xin tham gia (chỉ cho phép khi đang chờ duyệt)
        /// </summary>
        [HttpPut("my-join-requests/{id}")]
        public async Task<IActionResult> UpdateJoinRequest(string id, [FromBody] UpdateJoinRequestRequest request)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var joinRequest = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.Id == id && mg.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (joinRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy đơn xin tham gia"
                    });
                }

                // Chỉ cho phép sửa nếu đơn đang chờ duyệt
                if (joinRequest.IsApproved != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = joinRequest.IsApproved.Value
                            ? "Không thể sửa đơn đã được phê duyệt"
                            : "Không thể sửa đơn đã bị từ chối"
                    });
                }

                // Update thông tin
                joinRequest.Reason = request.Reason;
                joinRequest.Company = request.Company;
                joinRequest.Position = request.Position;
                joinRequest.UpdatedDate = DateTime.Now;

                _membershipGroupRepository.Update(joinRequest);
                await _unitOfWork.SaveChangesAsync();

                return Success(new
                {
                    message = "Cập nhật đơn xin tham gia thành công",
                    id = joinRequest.Id,
                    reason = joinRequest.Reason,
                    company = joinRequest.Company,
                    position = joinRequest.Position,
                    updatedDate = joinRequest.UpdatedDate
                }, "Cập nhật đơn xin tham gia thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating join request {Id}", id);
                return Error("Có lỗi xảy ra khi cập nhật đơn xin tham gia", 500);
            }
        }

        /// <summary>
        /// [MINI APP] Hủy đơn xin tham gia (chỉ cho phép khi đang chờ duyệt)
        /// </summary>
        [HttpDelete("my-join-requests/{id}")]
        public async Task<IActionResult> CancelJoinRequest(string id)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var joinRequest = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.Id == id && mg.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (joinRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy đơn xin tham gia"
                    });
                }

                // Chỉ cho phép hủy nếu đơn đang chờ duyệt
                if (joinRequest.IsApproved != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = joinRequest.IsApproved.Value
                            ? "Không thể hủy đơn đã được phê duyệt"
                            : "Không thể hủy đơn đã bị từ chối"
                    });
                }

                _membershipGroupRepository.Delete(joinRequest);
                await _unitOfWork.SaveChangesAsync();

                return Success(new
                {
                    message = "Hủy đơn xin tham gia thành công"
                }, "Hủy đơn xin tham gia thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling join request {Id}", id);
                return Error("Có lỗi xảy ra khi hủy đơn xin tham gia", 500);
            }
        }

        /// <summary>
        /// [MINI APP] Lấy danh sách hội nhóm có thể tham gia (chưa tham gia)
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableGroups()
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Get all active groups
                var allGroups = await _groupService.GetActiveGroupsAsync();

                // Get groups user already joined
                var joinedGroupIds = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.UserZaloId == userZaloId)
                    .Select(mg => mg.GroupId)
                    .ToListAsync();

                // Filter out joined groups and add isJoined status
                var availableGroups = allGroups.Where(g => !joinedGroupIds.Contains(g.Id))
                    .Select(g => new GroupDTO
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description,
                        Rule = g.Rule,
                        IsActive = g.IsActive,
                        MemberCount = 0, // Will be calculated separately if needed
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate,
                        IsJoined = false // Available groups are not joined by definition
                    }).ToList();

                return Success(availableGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available groups for {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return Error("Có lỗi xảy ra khi lấy danh sách hội nhóm", 500);
            }
        }

        /// <summary>
        /// Lấy danh sách hội nhóm đang hoạt động (cho dropdown, filter)
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveGroups()
        {
            try
            {
                // Lấy userZaloId từ JWT token (nếu có)
                var userZaloId = User.FindFirst("UserZaloId")?.Value;

                var groups = await _groupService.GetActiveGroupsAsync();

                // Nếu có userZaloId, kiểm tra trạng thái tham gia
                if (!string.IsNullOrEmpty(userZaloId))
                {
                    var joinedGroupIds = await _membershipGroupRepository.AsQueryable()
                        .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                        .Select(mg => mg.GroupId)
                        .ToHashSetAsync();

                    var groupsWithStatus = groups.Select(g => new GroupDTO
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description,
                        Rule = g.Rule,
                        IsActive = g.IsActive,
                        MemberCount = 0,
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate,
                        IsJoined = joinedGroupIds.Contains(g.Id)
                    }).ToList();

                    return Success(groupsWithStatus, "Lấy danh sách hội nhóm đang hoạt động thành công");
                }
                else
                {
                    var groupsWithStatus = groups.Select(g => new GroupDTO
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description,
                        Rule = g.Rule,
                        IsActive = g.IsActive,
                        MemberCount = 0,
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate,
                        IsJoined = false
                    }).ToList();

                    return Success(groupsWithStatus, "Lấy danh sách hội nhóm đang hoạt động thành công");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active groups");
                return Error("Có lỗi xảy ra khi lấy danh sách hội nhóm đang hoạt động", 500);
            }
        }

        #endregion
        private string GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var request = HttpContext.Request;
            if (request == null)
                return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{relativePath}";
        }

    }

}
