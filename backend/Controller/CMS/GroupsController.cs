using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;
using MiniAppGIBA.Service.Groups;
using MiniAppGIBA.Services.Dashboard.GroupDashboard;
using MiniAppGIBA.Services.Groups;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Services.Memberships;
using MiniAppGIBA.Exceptions;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    public class GroupsController : BaseCMSController
    {
        private readonly IGroupService _groupService;
        private readonly IGroupDashboardService _groupDashboardService;
        private readonly ILogger<GroupsController> _logger;
        private readonly IMembershipGroupService _membershipGroupService;
        private readonly IMembershipService _membershipService;
        private readonly IUnitOfWork _unitOfWork;

        public GroupsController(
            IGroupService groupService,
            IGroupDashboardService groupDashboardService,
            ILogger<GroupsController> logger,
            IMembershipGroupService membershipGroupService,
            IMembershipService membershipService,
            IUnitOfWork unitOfWork)
        {
            _groupService = groupService;
            _groupDashboardService = groupDashboardService;
            _logger = logger;
            _membershipGroupService = membershipGroupService;
            _membershipService = membershipService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Trang danh sách hội nhóm
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy thống kê cơ bản
                var statistics = await _groupService.GetGroupStatisticsAsync();

                ViewBag.TotalGroups = statistics.TotalGroups;
                ViewBag.ActiveGroups = statistics.ActiveGroups;
                ViewBag.InactiveGroups = statistics.InactiveGroups;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading groups index page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang quản lý hội nhóm");
                return View();
            }
        }

        /// <summary>
        /// API endpoint cho DataTable - lấy danh sách hội nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPage([FromQuery] GroupQueryParameters query)
        {
            try
            {
                // GIBA has full access to all groups - no filtering needed
                var groups = await _groupService.GetGroupsAsync(query, null, null, query.GroupType);

                var result = new
                {
                    data = groups.Items.Select(g => new
                    {
                        id = g.Id,
                        name = g.GroupName,
                        description = g.Description ?? "",
                        status = g.IsActive,
                        memberCount = g.MemberCount,
                        logo = g.Logo ?? "",
                        createdDate = g.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                        updatedDate = g.UpdatedDate.ToString("dd/MM/yyyy HH:mm")
                    }).ToList(),
                    totalItems = groups.TotalItems,
                    page = groups.Page,
                    pageSize = groups.PageSize,
                    totalPages = groups.TotalPages
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups page");
                return Json(new { data = new List<object>(), totalItems = 0 });
            }
        }

        /// <summary>
        /// API lấy tất cả groups (không phân trang) - dùng cho dropdown/select
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var query = new GroupQueryParameters
                {
                    PageSize = int.MaxValue,
                    Page = 1,
                    Status = true // Chỉ lấy groups active
                };

                // GIBA has full access - get all groups without filtering
                var result = await _groupService.GetGroupsAsync(query, null, null, null);

                return Json(new
                {
                    success = true,
                    data = result.Items.Select(g => new
                    {
                        id = g.Id,
                        name = g.GroupName,
                        description = g.Description ?? "",
                        isJoined = g.IsJoined,
                        joinStatus = g.JoinStatus,
                        joinStatusText = g.JoinStatusText,
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all groups");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách nhóm" });
            }
        }

        /// <summary>
        /// Trang tạo hội nhóm mới
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            ViewBag.IsActive = true;
            return View("Partials/_Groups", new CreateGroupRequest());
        }

        /// <summary>
        /// API endpoint để load form create cho modal
        /// </summary>
        [HttpGet]
        public IActionResult GetCreateForm()
        {
            ViewBag.IsEdit = false;
            ViewBag.IsActive = true;
            return View("Partials/_Groups", new CreateGroupRequest());
        }

        /// <summary>
        /// Xử lý tạo hội nhóm mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupRequest request)
        {
            try
            {
                // Only SUPER_ADMIN can create groups
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ SUPER_ADMIN mới có quyền tạo hội nhóm!");
                    return RedirectToAction(nameof(Index));
                }
                // Lấy IsActive từ form data
                var isActiveValue = Request.Form["IsActive"].FirstOrDefault();
                bool isActive = isActiveValue == "true" || isActiveValue == "on";

                if (!ModelState.IsValid)
                {
                    ViewBag.IsEdit = false;
                    ViewBag.IsActive = isActive;
                    return View("Partials/_Groups", request);
                }

                // Cập nhật IsActive vào request
                request.IsActive = isActive;

                await _groupService.CreateGroupAsync(request);

                SetSuccessMessage("Tạo hội nhóm thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                SetErrorMessage(ex.Message);
                ViewBag.IsEdit = false;
                ViewBag.IsActive = request.IsActive;
                return View("Partials/_Groups", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                SetErrorMessage("Có lỗi xảy ra khi tạo hội nhóm");
                ViewBag.IsEdit = false;
                ViewBag.IsActive = request.IsActive;
                return View("Partials/_Groups", request);
            }
        }

        /// <summary>
        /// Trang chỉnh sửa hội nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                // Only SUPER_ADMIN can edit groups
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ SUPER_ADMIN mới có quyền chỉnh sửa hội nhóm!");
                    return RedirectToAction(nameof(Index));
                }

                var groupDTO = await _groupService.GetGroupByIdAsync(id);
                if (groupDTO == null)
                {
                    SetErrorMessage("Không tìm thấy hội nhóm");
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IsEdit = true;
                ViewBag.IsActive = groupDTO.IsActive;
                ViewBag.GroupId = groupDTO.Id;
                ViewBag.GroupLogo = groupDTO.Logo; // Pass logo URL to view

                var createRequest = new CreateGroupRequest
                {
                    GroupName = groupDTO.GroupName,
                    Description = groupDTO.Description,
                    Rule = groupDTO.Rule
                };

                return View("Partials/_Groups", createRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group for edit {GroupId}", id);
                SetErrorMessage("Có lỗi xảy ra khi tải thông tin hội nhóm");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API endpoint để load form edit cho modal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEditForm(string id)
        {
            try
            {
                _logger.LogInformation("GetEditForm called with ID: {GroupId}", id);

                var groupDTO = await _groupService.GetGroupByIdAsync(id);
                if (groupDTO == null)
                {
                    _logger.LogWarning("Group not found with ID: {GroupId}", id);
                    return Json(new { success = false, message = "Không tìm thấy hội nhóm" });
                }

                _logger.LogInformation("Group found: {GroupName}, IsActive: {IsActive}",
                    groupDTO.GroupName, groupDTO.IsActive);

                ViewBag.IsEdit = true;
                ViewBag.IsActive = groupDTO.IsActive;
                ViewBag.GroupId = groupDTO.Id;

                var createRequest = new CreateGroupRequest
                {
                    GroupName = groupDTO.GroupName,
                    Description = groupDTO.Description,
                    Rule = groupDTO.Rule
                };

                _logger.LogInformation("CreateRequest prepared: {GroupName}, {Description}, {Rule}",
                    createRequest.GroupName, createRequest.Description, createRequest.Rule);

                var partialView = await Task.Run(() =>
                    View("Partials/_Groups", createRequest)
                );

                return partialView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group edit form {GroupId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form chỉnh sửa" });
            }
        }

        /// <summary>
        /// Xử lý cập nhật hội nhóm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateGroupRequest request)
        {
            // Lấy IsActive từ form data
            var isActiveValue = Request.Form["IsActive"].FirstOrDefault();
            bool isActive = isActiveValue == "true" || isActiveValue == "on";

            try
            {
                // Only SUPER_ADMIN can edit groups
                if (!IsSuperAdmin())
                {
                    SetErrorMessage("Chỉ SUPER_ADMIN mới có quyền chỉnh sửa hội nhóm!");
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.IsEdit = true;
                    ViewBag.IsActive = isActive;
                    ViewBag.GroupId = id;
                    return View("Partials/_Groups", request);
                }

                var updateRequest = new UpdateGroupRequest
                {
                    Id = id,
                    GroupName = request.GroupName,
                    Description = request.Description,
                    Rule = request.Rule,
                    IsActive = isActive
                };

                await _groupService.UpdateGroupAsync(id, updateRequest);

                SetSuccessMessage("Cập nhật hội nhóm thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                SetErrorMessage("Không tìm thấy hội nhóm");
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                SetErrorMessage(ex.Message);
                ViewBag.IsEdit = true;
                ViewBag.IsActive = isActive;
                ViewBag.GroupId = id;
                return View("Partials/_Groups", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId}", id);
                SetErrorMessage("Có lỗi xảy ra khi cập nhật hội nhóm");
                ViewBag.IsEdit = true;
                ViewBag.IsActive = isActive;
                ViewBag.GroupId = id;
                return View("Partials/_Groups", request);
            }
        }

        /// <summary>
        /// Trang chi tiết hội nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var groupDTO = await _groupService.GetGroupByIdAsync(id);
                if (groupDTO == null)
                {
                    SetErrorMessage("Không tìm thấy hội nhóm");
                    return RedirectToAction(nameof(Index));
                }

                return View(groupDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group details {GroupId}", id);
                SetErrorMessage("Có lỗi xảy ra khi tải thông tin hội nhóm");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xóa hội nhóm
        /// </summary>
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Delete group called with empty id");
                    return BadRequest(new { success = false, message = "ID hội nhóm không được để trống" });
                }

                _logger.LogInformation("Deleting group with id: {GroupId}", id);
                var result = await _groupService.DeleteGroupAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy hội nhóm" });
                }

                return Json(new { success = true, message = "Xóa hội nhóm thành công" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation when deleting group {GroupId}", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hội nhóm" });
            }
        }

        /// <summary>
        /// Thay đổi trạng thái hội nhóm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var result = await _groupService.ToggleGroupStatusAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy hội nhóm" });
                }

                return Json(new { success = true, message = "Thay đổi trạng thái hội nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling group status {GroupId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái hội nhóm" });
            }
        }

        /// <summary>
        /// Trang chi tiết hội nhóm với leaderboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Index");
                }

                // Lấy thông tin nhóm
                var group = await _groupService.GetGroupByIdAsync(id);
                if (group == null)
                {
                    SetErrorMessage("Không tìm thấy hội nhóm");
                    return RedirectToAction("Index");
                }

                // Lấy thống kê dashboard của nhóm
                var groupSummary = await _groupDashboardService.GetGroupDashboardSummaryAsync(id);

                ViewBag.Group = group;
                ViewBag.GroupSummary = groupSummary;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group detail page for group {GroupId}", id);
                SetErrorMessage("Có lỗi xảy ra khi tải trang chi tiết hội nhóm");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// API lấy leaderboard của nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupLeaderboard(string id, string period = "month", int limit = 20, string sortBy = "TotalRefs")
        {
            try
            {
                _logger.LogInformation("Getting group leaderboard for group {GroupId} with sortBy: {SortBy}", id, sortBy);

                var leaderboard = await _groupDashboardService.GetGroupLeaderboardAsync(id, period, limit, sortBy);

                _logger.LogInformation("Retrieved {Count} leaderboard entries for group {GroupId}", leaderboard.Count, id);

                return Json(new { success = true, data = leaderboard });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group leaderboard for group {GroupId} with sortBy: {SortBy}", id, sortBy);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải leaderboard" });
            }
        }

        /// <summary>
        /// API lấy top 3 thành viên của nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupTop3(string id, string period = "month")
        {
            try
            {
                var top3 = await _groupDashboardService.GetGroupTop3Async(id, period);
                return Json(new { success = true, data = top3 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group top 3 for group {GroupId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải top 3" });
            }
        }

        /// <summary>
        /// API lấy dữ liệu thống kê theo tháng của nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupMonthlyData(string id, int months = 12)
        {
            try
            {
                var monthlyData = await _groupDashboardService.GetGroupMonthlyRefDataAsync(id, months);
                return Json(new { success = true, data = monthlyData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group monthly data for group {GroupId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu tháng" });
            }
        }

        /// <summary>
        /// Load partial view for GROUP behavior rules management
        /// </summary>
        [HttpGet]
        public IActionResult GetBehaviorRulesContent(string groupId)
        {
            try
            {
                _logger.LogInformation("GetBehaviorRulesContent called for group: {GroupId}", groupId);

                if (string.IsNullOrEmpty(groupId))
                {
                    _logger.LogWarning("GetBehaviorRulesContent called with empty groupId");
                    return BadRequest("Group ID is required");
                }

                ViewBag.GroupId = groupId;
                return PartialView("Partials/_GroupBehaviorRules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading behavior rules content for group {GroupId}", groupId);
                return StatusCode(500, "Có lỗi xảy ra khi tải nội dung quy tắc ứng xử");
            }
        }

        /// <summary>
        /// API xóa thành viên khỏi nhóm
        /// GIBA has full access to delete members from any group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteMember([FromForm] string membershipGroupId)
        {
            try
            {
                if (string.IsNullOrEmpty(membershipGroupId))
                {
                    return Json(new { success = false, message = "ID không được để trống" });
                }

                // Only GIBA can delete members
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa thành viên này!" });
                }

                // Get membership group to verify it exists
                var membershipGroup = await _membershipGroupService.GetMembershipGroupByIdAsync(membershipGroupId);
                if (membershipGroup == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thành viên trong nhóm" });
                }

                // Delete membership group
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var mg = await membershipGroupRepo.FindByIdAsync(membershipGroupId);
                if (mg != null)
                {
                    membershipGroupRepo.Delete(mg);
                    await _unitOfWork.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Xóa thành viên khỏi nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting member from group {MembershipGroupId}", membershipGroupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa thành viên" });
            }
        }

        /// <summary>
        /// API lấy danh sách thành viên của nhóm (cho CMS)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupMembers(string groupId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return Json(new { success = false, message = "Group ID không được để trống" });
                }

                var query = new PendingApprovalQueryParameters
                {
                    GroupId = groupId,
                    ShouldFilterByApprovalStatus = true, // Chỉ lấy thành viên đã duyệt
                    IsApproved = true, // Chỉ lấy IsApproved == true
                    Page = 1,
                    PageSize = int.MaxValue
                };

                var allowedGroupIds = GetUserGroupIdsOrNull();
                var result = await _membershipGroupService.GetMembershipGroupsAsync(query, allowedGroupIds);

                // Lấy thông tin phone number và membershipId từ Membership
                var userZaloIds = result.Items
                    .Where(mg => !string.IsNullOrEmpty(mg.UserZaloId))
                    .Select(mg => mg.UserZaloId!)
                    .ToList();
                
                // Query để lấy phone number và membershipId
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var memberships = await membershipRepo.AsQueryable()
                    .Where(m => userZaloIds.Contains(m.UserZaloId) && m.IsDelete != true && !string.IsNullOrEmpty(m.UserZaloId))
                    .Select(m => new { m.UserZaloId, m.PhoneNumber, m.Id })
                    .ToListAsync();
                
                var phoneNumberDict = memberships
                    .Where(m => !string.IsNullOrEmpty(m.UserZaloId))
                    .ToDictionary(m => m.UserZaloId!, m => m.PhoneNumber ?? "");
                
                var membershipIdDict = memberships
                    .Where(m => !string.IsNullOrEmpty(m.UserZaloId))
                    .ToDictionary(m => m.UserZaloId!, m => m.Id);

                return Json(new
                {
                    success = true,
                    data = result.Items.Select(mg => new
                    {
                        id = mg.Id, // membershipGroupId
                        membershipId = !string.IsNullOrEmpty(mg.UserZaloId) && membershipIdDict.ContainsKey(mg.UserZaloId) ? membershipIdDict[mg.UserZaloId] : "",
                        userZaloId = mg.UserZaloId,
                        memberName = mg.MemberName,
                        phoneNumber = !string.IsNullOrEmpty(mg.UserZaloId) && phoneNumberDict.ContainsKey(mg.UserZaloId) ? phoneNumberDict[mg.UserZaloId] : "",
                        company = mg.Company ?? "",
                        position = mg.Position ?? "",
                        groupPosition = mg.GroupPosition ?? "",
                        sortOrder = mg.SortOrder ?? 0,
                        isApproved = mg.IsApproved,
                        statusText = mg.StatusText,
                        statusClass = mg.StatusClass,
                        createdDate = mg.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                        approvedDate = mg.ApprovedDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
                        zaloAvatar = mg.ZaloAvatar ?? ""
                    }).OrderBy(m => m.sortOrder).ThenBy(m => m.createdDate).ToList(),
                    totalItems = result.TotalItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members for group {GroupId}", groupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách thành viên" });
            }
        }

        /// <summary>
        /// API cập nhật thứ tự và chức vụ thành viên trong nhóm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateMembersOrderAndPosition([FromBody] UpdateMembersOrderRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.GroupId))
                {
                    return Json(new { success = false, message = "Group ID không được để trống" });
                }

                if (request.Members == null || !request.Members.Any())
                {
                    return Json(new { success = false, message = "Danh sách thành viên không được để trống" });
                }

                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var allowedGroupIds = GetUserGroupIdsOrNull();

                // Kiểm tra quyền truy cập nhóm
                var group = await _unitOfWork.GetRepository<Group>()
                    .GetFirstOrDefaultAsync(g => g.Id == request.GroupId);
                
                if (group == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hội nhóm" });
                }

                if (allowedGroupIds != null && !allowedGroupIds.Contains(request.GroupId))
                {
                    return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này" });
                }

                // Cập nhật từng thành viên
                foreach (var memberData in request.Members)
                {
                    var membershipGroup = await membershipGroupRepo.GetFirstOrDefaultAsync(
                        mg => mg.Id == memberData.MembershipGroupId && mg.GroupId == request.GroupId);

                    if (membershipGroup != null)
                    {
                        membershipGroup.SortOrder = memberData.SortOrder;
                        membershipGroup.GroupPosition = memberData.GroupPosition?.Trim();
                        membershipGroup.UpdatedDate = DateTime.Now;
                        membershipGroupRepo.Update(membershipGroup);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thứ tự và chức vụ thành viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating members order and position for group {GroupId}", request?.GroupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thứ tự thành viên" });
            }
        }

        /// <summary>
        /// API tìm kiếm thành viên để thêm vào nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchMembers(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                var members = await _membershipService.SearchMembersForGroupAsync(searchTerm);

                return Json(new
                {
                    success = true,
                    data = members.Select(m => new
                    {
                        id = m.Id,
                        userZaloId = m.UserZaloId,
                        fullname = m.Fullname,
                        phoneNumber = m.PhoneNumber,
                        zaloAvatar = m.ZaloAvatar,
                        roleId = m.RoleId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching members with term: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm thành viên" });
            }
        }

        /// <summary>
        /// API thêm thành viên vào nhóm
        /// GIBA has full access to add members to any group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddMemberToGroup([FromForm] string groupId, [FromForm] string userZaloId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userZaloId))
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                // Only GIBA can add members to groups
                if (!IsSuperAdmin())
                {
                    return Json(new { success = false, message = "Bạn không có quyền thêm thành viên vào nhóm này!" });
                }

                var result = await _membershipGroupService.AddMemberToGroupAsync(groupId, userZaloId);

                return Json(new { success = true, message = "Thêm thành viên vào nhóm thành công", data = result });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member {UserZaloId} to group {GroupId}", userZaloId, groupId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm thành viên vào nhóm" });
            }
        }

    }

    public class UpdateMembersOrderRequest
    {
        public string GroupId { get; set; } = string.Empty;
        public List<MemberOrderData> Members { get; set; } = new List<MemberOrderData>();
    }

    public class MemberOrderData
    {
        public string MembershipGroupId { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? GroupPosition { get; set; }
    }
}
