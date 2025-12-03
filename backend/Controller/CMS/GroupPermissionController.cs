using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Models.DTOs.Admins;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Service.Groups;
using MiniAppGIBA.Services.Admins;
using MiniAppGIBA.Services.Memberships;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Constants;

namespace MiniAppGIBA.Controller.CMS
{
    /// <summary>
    /// DEPRECATED: This controller is no longer needed as the role system has been simplified.
    /// GIBA admin now has full access to all groups without needing GroupPermission assignments.
    /// This controller is kept for backward compatibility but should not be used for new features.
    /// </summary>
    [Obsolete("GroupPermissionController is deprecated. GIBA admin has full access to all groups without needing GroupPermission assignments.")]
    [Authorize(Roles = CTRole.GIBA)]
    public class GroupPermissionController : BaseCMSController
    {
        private readonly IGroupPermissionService _groupPermissionService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMembershipService _membershipService;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<GroupPermissionController> _logger;

        public GroupPermissionController(
            IGroupPermissionService groupPermissionService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager,
            IMembershipService membershipService,
            IActivityLogService activityLogService,
            ILogger<GroupPermissionController> logger)
        {
            _groupPermissionService = groupPermissionService;
            _groupService = groupService;
            _userManager = userManager;
            _membershipService = membershipService;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        /// <summary>
        /// Trang quản lý phân quyền nhóm
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var permissions = await _groupPermissionService.GetAllWithDetailsAsync();
                return View(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading group permissions page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang phân quyền nhóm");
                return View(new List<GroupPermissionDto>());
            }
        }

        /// <summary>
        /// Trang phân quyền nhóm cho ADMIN
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AssignGroups(string adminId)
        {
            try
            {
                var admin = await _membershipService.GetMembershipByIdAsync(adminId);
                if (admin == null)
                {
                    SetErrorMessage("Không tìm thấy tài khoản ADMIN");
                    return RedirectToAction("Index", "AdminManagement");
                }

                var allGroupsQuery = new GroupQueryParameters { PageSize = int.MaxValue };
                var allGroupsResult = await _groupService.GetAllGroupsAsync(allGroupsQuery);
                var assignedGroupIds = await _groupPermissionService.GetGroupIdsByUserIdAsync(adminId);

                ViewBag.AdminId = adminId;
                ViewBag.AdminName = admin.Fullname;
                ViewBag.AdminEmail = admin.PhoneNumber; // Email removed, using PhoneNumber
                ViewBag.AllGroups = allGroupsResult.Items;
                ViewBag.AssignedGroupIds = assignedGroupIds;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assign groups page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang phân quyền");
                return RedirectToAction("Index", "AdminManagement");
            }
        }

        /// <summary>
        /// Phân quyền nhóm cho ADMIN
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGroup(CreateGroupPermissionDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                // Service sẽ tự check và xử lý duplicate (nếu đã tồn tại nhưng IsActive = false sẽ active lại)
                var permission = await _groupPermissionService.AssignGroupToAdminAsync(model);

                // Log activity
                var currentUserId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var admin = await _membershipService.GetMembershipByIdAsync(model.UserId);
                    await _activityLogService.LogActivityAsync(new CreateActivityLogDto
                    {
                        AccountId = currentUserId,
                        ActionType = "ASSIGN_GROUPS",
                        Description = $"Assigned group to ADMIN: {admin?.Fullname}",
                        TargetEntity = "GroupPermission",
                        TargetId = permission.Id
                    });
                }

                return Json(new { success = true, message = "Phân quyền nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning group to admin");
                return Json(new { success = false, message = "Có lỗi xảy ra khi phân quyền nhóm" });
            }
        }

        /// <summary>
        /// Phân quyền nhiều nhóm cho ADMIN (batch assignment)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AssignGroups(string userId, List<string> groupIds)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "UserId không hợp lệ" });
                }

                var admin = await _membershipService.GetMembershipByIdAsync(userId);
                if (admin == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản ADMIN" });
                }

                // Xóa tất cả permissions cũ của admin này
                var existingPermissions = await _groupPermissionService.GetByUserIdAsync(userId);
                foreach (var perm in existingPermissions)
                {
                    await _groupPermissionService.RevokeGroupFromAdminAsync(perm.Id);
                }

                // Thêm permissions mới
                int assignedCount = 0;
                if (groupIds != null && groupIds.Any())
                {
                    foreach (var groupId in groupIds)
                    {
                        var dto = new CreateGroupPermissionDto
                        {
                            UserId = userId,
                            GroupId = groupId
                        };
                        await _groupPermissionService.AssignGroupToAdminAsync(dto);
                        assignedCount++;
                    }
                }

                // Log activity
                var currentUserId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await _activityLogService.LogActivityAsync(new CreateActivityLogDto
                    {
                        AccountId = currentUserId,
                        ActionType = "ASSIGN_GROUPS",
                        Description = $"Assigned {assignedCount} groups to ADMIN: {admin.Fullname ?? admin.PhoneNumber}",
                        TargetEntity = "GroupPermission",
                        TargetId = userId
                    });
                }

                return Json(new { success = true, message = $"Đã phân quyền {assignedCount} nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning groups to admin");
                return Json(new { success = false, message = "Có lỗi xảy ra khi phân quyền nhóm" });
            }
        }

        /// <summary>
        /// Thu hồi quyền nhóm của ADMIN
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokePermission(string permissionId)
        {
            try
            {
                var success = await _groupPermissionService.RevokeGroupFromAdminAsync(permissionId);
                if (!success)
                {
                    return Json(new { success = false, message = "Không tìm thấy quyền nhóm" });
                }

                // Log activity
                var currentUserId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await _activityLogService.LogActivityAsync(new CreateActivityLogDto
                    {
                        AccountId = currentUserId,
                        ActionType = "REVOKE_GROUP_PERMISSION",
                        Description = $"Revoked group permission",
                        TargetEntity = "GroupPermission",
                        TargetId = permissionId
                    });
                }

                return Json(new { success = true, message = "Thu hồi quyền nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking group permission");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thu hồi quyền" });
            }
        }

        /// <summary>
        /// Xóa hoàn toàn permission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermission(string permissionId)
        {
            try
            {
                var success = await _groupPermissionService.DeletePermissionAsync(permissionId);
                if (!success)
                {
                    return Json(new { success = false, message = "Không tìm thấy quyền nhóm" });
                }

                // Log activity
                var currentUserId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await _activityLogService.LogActivityAsync(new CreateActivityLogDto
                    {
                        AccountId = currentUserId,
                        ActionType = "DELETE_GROUP_PERMISSION",
                        Description = $"Deleted group permission",
                        TargetEntity = "GroupPermission",
                        TargetId = permissionId
                    });
                }

                return Json(new { success = true, message = "Xóa quyền nhóm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group permission");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa quyền" });
            }
        }

        /// <summary>
        /// API lấy danh sách nhóm của ADMIN
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAdminGroups(string adminId)
        {
            try
            {
                var groupIds = await _groupPermissionService.GetGroupIdsByUserIdAsync(adminId);
                return Json(new { success = true, data = groupIds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin groups");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// API lấy danh sách GroupPermissions của ADMIN (backward compatibility)
        /// </summary>
        [HttpGet("GetByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            try
            {
                var permissions = await _groupPermissionService.GetByUserIdAsync(userId);
                return Json(new { success = true, data = permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}

