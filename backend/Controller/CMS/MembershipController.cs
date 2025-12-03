using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Services.Memberships;
using MiniAppGIBA.Models.Queries.Memberships;
using MiniAppGIBA.Base.Interface;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("Membership")]
    public class MembershipController : BaseCMSController
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(
            IMembershipService membershipService,
            ILogger<MembershipController> logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            ViewBag.IsCreate = true;
            return PartialView("Partials/_MembershipCreateForm");
        }

        [HttpGet("Edit/{id?}")]
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit([FromRoute] string? id, [FromQuery(Name = "id")] string? queryId)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Forbid();
                }

                // Hỗ trợ cả route parameter và query string
                var membershipId = id ?? queryId;
                if (string.IsNullOrEmpty(membershipId))
                {
                    return NotFound();
                }

                var membership = await _membershipService.GetMembershipByIdAsync(membershipId);
                if (membership == null)
                {
                    _logger.LogWarning("Membership not found for ID: {MembershipId}", membershipId);
                    return NotFound();
                }

                _logger.LogInformation("Loading edit form for membership: {MembershipId}, Fullname: {Fullname}",
                    membership.Id, membership.Fullname);

                ViewBag.MembershipId = membership.Id;

                var request = new UpdateMembershipRequest
                {
                    UserZaloName = membership.UserZaloName,
                    Fullname = membership.Fullname ?? string.Empty,
                    ZaloAvatar = membership.ZaloAvatar,
                    PhoneNumber = membership.PhoneNumber,
                    RoleId = membership.RoleId
                };

                _logger.LogInformation("UpdateMembershipRequest created - Fullname: {Fullname}, PhoneNumber: {PhoneNumber}",
                    request.Fullname, request.PhoneNumber);

                // Pass additional membership data to ViewBag for readonly fields
                ViewBag.MembershipData = new
                {
                    PhoneNumber = membership.PhoneNumber ?? "Chưa cập nhật",
                    UserZaloName = membership.UserZaloName ?? "Chưa cập nhật",
                    UserZaloId = membership.UserZaloId ?? ""
                };

                return PartialView("Partials/_MembershipForm", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading membership edit form for ID: {MembershipId}", id ?? queryId);
                // Return JSON error response
                return Json(new { success = false, message = "Không thể tải form chỉnh sửa thành viên" });
            }
        }

        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage(int page = 1, int pageSize = 10, string? keyword = null)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var queryParameters = new MembershipQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Keyword = keyword
                };

                // Get allowed group IDs for ADMIN users (null for SUPER_ADMIN)
                var allowedGroupIds = GetUserGroupIdsOrNull();
                var result = await _membershipService.GetMembershipsAsync(queryParameters, allowedGroupIds);

                var responseData = result.Items.Select(m => new
                {
                    id = m.Id,
                    userZaloId = m.UserZaloId,
                    fullname = m.Fullname,
                    phoneNumber = m.PhoneNumber,
                    zaloAvatar = m.ZaloAvatar,
                    createdDate = m.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    // Store full data for edit form
                    formattedData = new
                    {
                        fullname = m.Fullname,
                        userZaloName = m.UserZaloName,
                        phoneNumber = m.PhoneNumber,
                        zaloAvatar = m.ZaloAvatar,
                        roleId = m.RoleId
                    }
                }).ToList();
                return Json(new
                {
                    success = true,
                    data = responseData,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memberships");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateMembershipRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Validate phone number is required
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return Json(new { success = false, message = "Số điện thoại là bắt buộc" });
                }

                await _membershipService.CreateMembershipAsync(request);
                return Json(new { success = true, message = "Tạo tài khoản thành công!" });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo tài khoản" });
            }
        }

        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] string id, [FromForm] UpdateMembershipRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                await _membershipService.UpdateMembershipAsync(id, request);
                return Json(new { success = true, message = "Cập nhật thành viên thành công!" });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership {MembershipId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                await _membershipService.DeleteMembershipAsync(id);
                return Json(new { success = true, message = "Xóa thành viên thành công!" });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting membership {MembershipId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
