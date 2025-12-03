using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Models.DTOs.Admins;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Services.Admins;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Services.Memberships;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Helpers;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(Roles = CTRole.GIBA)]
    [Route("AdminManagement")]
    public class AdminManagementController : BaseCMSController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGroupPermissionService _groupPermissionService;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<AdminManagementController> _logger;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IRepository<GroupPermission> _groupPermissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AdminManagementController(
            UserManager<ApplicationUser> userManager,
            IGroupPermissionService groupPermissionService,
            IActivityLogService activityLogService,
            ILogger<AdminManagementController> logger,
            IMembershipService membershipService,
            IRepository<Roles> rolesRepository,
            IRepository<Membership> membershipRepository,
            IRepository<GroupPermission> groupPermissionRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _groupPermissionService = groupPermissionService;
            _activityLogService = activityLogService;
            _logger = logger;
            _membershipService = membershipService;
            _rolesRepository = rolesRepository;
            _membershipRepository = membershipRepository;
            _groupPermissionRepository = groupPermissionRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Trang danh sách ADMIN accounts
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var superAdminUsers = await _membershipService.GetMembershipsByRoleAsync(CTRole.GIBA);

                var adminDtos = superAdminUsers.Select(a => new
                {
                    a.Id,
                    a.Username,
                    a.Fullname,
                    a.PhoneNumber,
                    a.CreatedDate
                }).ToList();

                ViewBag.TotalAdmins = adminDtos.Count;

                return View(adminDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin management page");
                SetErrorMessage("Có lỗi xảy ra khi tải trang quản lý ADMIN");
                return View(new List<object>());
            }
        }

        /// <summary>
        /// API lấy chi tiết admin theo ID
        /// </summary>
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var admin = await _membershipService.GetMembershipByIdAsync(id);
                if (admin == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy admin" });
                }

                var userRole = !string.IsNullOrEmpty(admin.RoleId)
                    ? await _membershipService.GetRoleNameAsync(admin.RoleId)
                    : string.Empty;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = admin.Id,
                        username = admin.Username,
                        fullName = admin.Fullname,
                        phoneNumber = admin.PhoneNumber,
                        role = userRole,
                        createdDate = admin.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin by ID: {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thông tin admin" });
            }
        }

        /// <summary>
        /// API lấy danh sách ADMIN với phân trang (cho DataTable)
        /// </summary>
        [HttpPost("GetPage")]
        public async Task<IActionResult> GetPage(int draw, int page = 1, int length = 10, string keyword = "", string role = "")
        {
            try
            {
                var superAdminUsers = await _membershipService.GetMembershipsByRoleAsync(CTRole.GIBA);
                var allUsers = superAdminUsers;

                var totalRecords = allUsers.Count();

                // Pagination
                var pagedAdmins = allUsers
                    .OrderByDescending(a => a.CreatedDate)
                    .Skip((page - 1) * length)
                    .Take(length)
                    .ToList();

                // Get roles for each admin
                var result = new List<object>();
                foreach (var admin in pagedAdmins)
                {
                    var userRole = !string.IsNullOrEmpty(admin.RoleId)
                        ? await _membershipService.GetRoleNameAsync(admin.RoleId)
                        : string.Empty;
                    result.Add(new
                    {
                        admin.Id,
                        admin.Username,
                        fullName = admin.Fullname,
                        phoneNumber = admin.PhoneNumber,
                        role = userRole,
                        createdDate = admin.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin page");
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = "Có lỗi xảy ra khi tải danh sách ADMIN"
                });
            }
        }

        /// <summary>
        /// Tạo ADMIN account mới
        /// Nếu user đã tồn tại (check-user-admin có data) thì fill và gọi Update
        /// Nếu không có thì tạo mới
        /// </summary>
        [HttpPost("Create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateAdminDto model)
{
    try
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}" });
        }
        model.PhoneNumber = PhoneNumberHandler.FixFormatPhoneNumber(model.PhoneNumber);

        // Check if user already exists (use Select to avoid NULL mapping issues)
        var existingMembershipData = await _membershipRepository.AsQueryable()
            .Where(m => m.PhoneNumber == model.PhoneNumber && m.IsDelete != true)
            .Select(m => new
            {
                m.Id,
                m.PhoneNumber,
                m.Fullname,
                m.RoleId
            })
            .FirstOrDefaultAsync();

        if (existingMembershipData != null && existingMembershipData.RoleId == null)
        {
            // Load full entity only when needed for update
            var existingMembership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == existingMembershipData.Id && m.IsDelete != true);
            if (existingMembership == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thành viên" });
            }
            // User exists but not admin yet - Update to admin
            // Get role by name
            var role = await _rolesRepository.AsQueryable()
                .FirstOrDefaultAsync(r => r.Name == model.Role);
            if (role == null)
            {
                return Json(new { success = false, message = $"Không tìm thấy vai trò: {model.Role}" });
            }

            // Update membership
            var updateRequest = new UpdateMembershipRequest
            {
                Fullname = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            await _membershipService.UpdateMembershipAsync(existingMembership.Id, updateRequest);

            // Update username, role, and password
            existingMembership.Username = model.Username;
            existingMembership.RoleId = role.Id;
            existingMembership.Password = AuthHelper.HashPassword(model.Password);
            _membershipRepository.Update(existingMembership);
            await _unitOfWork.SaveChangesAsync();

            // Log activity
            var currentUserId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                await _activityLogService.LogActivityAsync(new CreateActivityLogDto
                {
                    AccountId = currentUserId,
                    ActionType = "CREATE_ADMIN",
                    Description = $"Promoted existing user to {model.Role} account: {model.PhoneNumber}",
                    TargetEntity = "Membership",
                    TargetId = existingMembership.Id
                });
            }

            return Json(new
            {
                success = true,
                message = $"Nâng cấp tài khoản {model.Role} thành công",
                data = new { id = existingMembership.Id, phoneNumber = existingMembership.PhoneNumber, fullName = model.FullName }
            });
        }
        else if (existingMembershipData != null && existingMembershipData.RoleId != null)
        {
            // User is already an admin
            return Json(new { success = false, message = "Thành viên này đã là admin! Vui lòng đăng nhập với tài khoản admin" });
        }

        // User doesn't exist - Create new admin membership
        var membershipDto = await _membershipService.CreateAdminMembershipAsync(
            model.FullName,
            model.Username,
            model.Password,
            model.PhoneNumber,
            model.Role
        );

        // Log activity
        var currentUserId2 = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId2))
        {
            await _activityLogService.LogActivityAsync(new CreateActivityLogDto
            {
                AccountId = currentUserId2,
                ActionType = "CREATE_ADMIN",
                Description = $"Created {model.Role} account: {membershipDto.PhoneNumber}",
                TargetEntity = "Membership",
                TargetId = membershipDto.Id
            });
        }

        return Json(new
        {
            success = true,
            message = $"Tạo tài khoản {model.Role} thành công",
            data = new { id = membershipDto.Id, phoneNumber = membershipDto.PhoneNumber, fullName = membershipDto.Fullname }
        });
    }
    catch (CustomException ex)
    {
        _logger.LogWarning(ex, "Error creating admin account: {Message}", ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating admin account");
        return Json(new { success = false, message = "Có lỗi xảy ra khi tạo tài khoản ADMIN" });
    }
}

/// <summary>
/// Cập nhật thông tin ADMIN
/// </summary>
[HttpPost("Edit")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit([FromForm] UpdateAdminDto model)
{
    try
    {
        _logger.LogInformation($"Edit Admin - Id: {model.Id}, FullName: {model.FullName}, PhoneNumber: {model.PhoneNumber}");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning($"Model validation failed: {string.Join(", ", errors)}");
            return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}" });
        }

        var user = await _membershipService.GetMembershipByIdAsync(model.Id);
        if (user == null)
        {
            return Json(new { success = false, message = "Không tìm thấy tài khoản ADMIN" });
        }

        // Update role if provided
        string? newRoleId = null;
        if (!string.IsNullOrEmpty(model.Role))
        {
            // Get role by name
            var role = await _rolesRepository.AsQueryable()
                .FirstOrDefaultAsync(r => r.Name == model.Role);
            if (role != null)
            {
                newRoleId = role.Id;
            }
        }

        // Update membership via service
        var request = new UpdateMembershipRequest
        {
            Fullname = model.FullName,
            PhoneNumber = model.PhoneNumber
        };

        // Update membership
        var updatedUser = await _membershipService.UpdateMembershipAsync(model.Id, request);

        // Update username and role if provided
        var membership = await _membershipRepository.AsQueryable()
            .FirstOrDefaultAsync(m => m.Id == model.Id && m.IsDelete != true);
        if (membership != null)
        {
            if (!string.IsNullOrEmpty(model.Username))
            {
                // Check if username already exists (excluding current user)
                var existingUsername = await _membershipRepository.GetFirstOrDefaultAsync(
                    m => m.Username == model.Username && m.Id != model.Id && m.IsDelete != true);
                if (existingUsername != null)
                {
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại trong hệ thống" });
                }
                membership.Username = model.Username;
            }

            if (!string.IsNullOrEmpty(newRoleId))
            {
                membership.RoleId = newRoleId;
            }

            _membershipRepository.Update(membership);
            await _unitOfWork.SaveChangesAsync();
        }

        // Log activity
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            await _activityLogService.LogActivityAsync(new CreateActivityLogDto
            {
                AccountId = currentUserId,
                ActionType = "UPDATE_ADMIN",
                Description = $"Updated ADMIN account: {user.PhoneNumber}",
                TargetEntity = "ApplicationUser",
                TargetId = user.Id
            });
        }

        return Json(new
        {
            success = true,
            message = $"Cập nhật tài khoản {user.PhoneNumber} thành công",
            data = new { id = user.Id, phoneNumber = user.PhoneNumber, fullName = user.Fullname }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating admin account");
        return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật tài khoản" });
    }
}

/// <summary>
/// Xóa ADMIN account
/// Chỉ xóa role, username, password và group permissions
/// Nếu tài khoản không có thông tin gì khác (do admin tạo) thì xóa luôn membership
/// </summary>
[HttpPost("Delete/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(string id)
{
    try
    {
        var membership = await _membershipRepository.AsQueryable()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsDelete != true);
        if (membership == null)
        {
            return Json(new { success = false, message = "Không tìm thấy tài khoản ADMIN" });
        }

        // Get role name to check if it's Group role
        string? roleName = null;
        if (!string.IsNullOrEmpty(membership.RoleId))
        {
            var role = await _rolesRepository.FindByIdAsync(membership.RoleId);
            roleName = role?.Name;
        }

        // 1. Xóa role, username, password
        membership.RoleId = null;
        membership.Username = null;
        membership.Password = null;
        _membershipRepository.Update(membership);

        // 2. Xóa group permissions nếu role là Group
        if (roleName == CTRole.Group)
        {
            var groupPermissions = await _groupPermissionRepository.AsQueryable()
                .Where(gp => gp.UserId == id && gp.IsActive)
                .ToListAsync();

            foreach (var permission in groupPermissions)
            {
                await _groupPermissionService.DeletePermissionAsync(permission.Id);
            }
        }

        // 3. Kiểm tra xem membership có thông tin gì khác không
        // - UserZaloId có phải GUID không (admin tạo) hay là số từ Zalo (user thật)
        // - Có refs không (RefFrom hoặc RefTo)
        // - Có membership groups không
        // - Có event registrations không
        bool isAdminCreated = false;
        if (!string.IsNullOrEmpty(membership.UserZaloId))
        {
            // Check if UserZaloId is a GUID (admin created) or a number (real user from Zalo)
            isAdminCreated = Guid.TryParse(membership.UserZaloId, out _);
        }

        // Check for other data regardless of whether it's admin created or not
        var hasRefs = await _unitOfWork.Context.Refs
            .AnyAsync(r => r.RefFrom == membership.UserZaloId || r.RefTo == membership.UserZaloId);

        var hasMembershipGroups = await _unitOfWork.Context.MembershipGroups
            .AnyAsync(mg => mg.UserZaloId == membership.UserZaloId);

        var hasEventRegistrations = await _unitOfWork.Context.EventRegistrations
            .AnyAsync(er => er.UserZaloId == membership.UserZaloId);

        // Check if has other profile data (not just admin fields)
        bool hasProfileData = !string.IsNullOrEmpty(membership.ZaloAvatar);

        bool hasOtherData = hasRefs || hasMembershipGroups || hasEventRegistrations || hasProfileData;

        // 4. Nếu không có thông tin gì khác (admin created và không có data) thì xóa luôn membership
        if (isAdminCreated && !hasOtherData)
        {
            _membershipRepository.Delete(membership);
        }

        await _unitOfWork.SaveChangesAsync();

        // Log activity
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            await _activityLogService.LogActivityAsync(new CreateActivityLogDto
            {
                AccountId = currentUserId,
                ActionType = "DELETE_ADMIN",
                Description = $"Deleted ADMIN account: {membership.PhoneNumber}",
                TargetEntity = "Membership",
                TargetId = membership.Id
            });
        }

        var message = isAdminCreated && !hasOtherData
            ? $"Xóa tài khoản {membership.PhoneNumber} thành công"
            : $"Đã xóa quyền admin của tài khoản {membership.PhoneNumber}";

        return Json(new { success = true, message = message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting admin account");
        return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài khoản" });
    }
}

/// <summary>
/// Reset password cho ADMIN
/// </summary>
[HttpPost("ResetPassword")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(string id, string newPassword)
{
    try
    {
        var user = await _membershipService.GetMembershipByIdAsync(id);
        if (user == null)
        {
            return Json(new { success = false, message = "Không tìm thấy tài khoản ADMIN" });
        }

        // var token = await _membershipService.GeneratePasswordResetTokenAsync(user.Id);  
        var result = await _membershipService.ResetPasswordAsync(user.Id, newPassword);

        if (!result)
        {
            return Json(new { success = false, message = $"Reset password thất bại" });
        }

        // Log activity
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            await _activityLogService.LogActivityAsync(new CreateActivityLogDto
            {
                AccountId = currentUserId,
                ActionType = "RESET_PASSWORD",
                Description = $"Reset password for ADMIN: {user.PhoneNumber}",
                TargetEntity = "ApplicationUser",
                TargetId = user.Id
            });
        }

        return Json(new { success = true, message = $"Reset password cho {user.PhoneNumber} thành công" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error resetting admin password");
        return Json(new { success = false, message = "Có lỗi xảy ra khi reset password" });
    }
}


    }
}
