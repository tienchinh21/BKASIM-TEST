using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Services.Memberships;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("MembershipApproval")]
    public class MembershipApprovalController : BaseCMSController
    {
        private readonly IMembershipApprovalService _membershipApprovalService;
        private readonly IMembershipService _membershipService;
        private readonly ILogger<MembershipApprovalController> _logger;

        public MembershipApprovalController(
            IMembershipApprovalService membershipApprovalService,
            IMembershipService membershipService,
            ILogger<MembershipApprovalController> logger)
        {
            _membershipApprovalService = membershipApprovalService;
            _membershipService = membershipService;
            _logger = logger;
        }


        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? keyword = null, string? status = null)
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                IEnumerable<Membership> result;
                int totalPages = 1;

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    byte approvalStatus = byte.Parse(status);
                    switch (approvalStatus)
                    {
                        case 0: // Chờ phê duyệt
                            var pendingResult = await _membershipApprovalService.GetPendingMembershipsAsync(page, pageSize, keyword);
                            result = pendingResult.Items;
                            totalPages = pendingResult.TotalPages;
                            break;
                        case 1: // Đã phê duyệt
                            var approvedResult = await _membershipApprovalService.GetApprovedMembershipsAsync(page, pageSize, keyword);
                            result = approvedResult.Items;
                            totalPages = approvedResult.TotalPages;
                            break;
                        case 2: // Bị từ chối
                            var rejectedResult = await _membershipApprovalService.GetRejectedMembershipsAsync(page, pageSize, keyword);
                            result = rejectedResult.Items;
                            totalPages = rejectedResult.TotalPages;
                            break;
                        default:
                            var defaultResult = await _membershipApprovalService.GetAllMembershipsAsync(page, pageSize, keyword);
                            result = defaultResult.Items;
                            totalPages = defaultResult.TotalPages;
                            break;
                    }
                }
                else
                {
                    // Tất cả trạng thái
                    var allResult = await _membershipApprovalService.GetAllMembershipsAsync(page, pageSize, keyword);
                    result = allResult.Items;
                    totalPages = allResult.TotalPages;
                }

                ViewBag.Keyword = keyword;
                ViewBag.Status = status;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                
                _logger.LogInformation("Loaded {Count} memberships with status filter: {Status}", result.Count(), status ?? "pending");
                return View(result.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading memberships with status: {Status}", status);
                return View(new List<Membership>());
            }
        }

        /// <summary>
        /// Trang chi tiết thành viên
        /// </summary>
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                _logger.LogInformation("Detail called with id: {Id}", id);

                var membership = await _membershipApprovalService.GetMembershipDetailAsync(id);
                if (membership == null)
                {
                    _logger.LogWarning("Membership not found for id: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("Found Membership with UserZaloId: {UserZaloId}", membership.UserZaloId);

                // Get full membership details using UserZaloId
                var fullMembership = await _membershipService.GetMembershipByUserZaloIdAsync(membership.UserZaloId);
                
                _logger.LogInformation("FullMembership result: {FullMembership}", fullMembership != null ? "Found" : "Not found");

                ViewBag.FullMembership = fullMembership; // Pass full membership data

                return View(membership);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Detail for id: {Id}", id);
                return NotFound();
            }
        }

        /// <summary>
        /// API: Phê duyệt thành viên
        /// </summary>
        [HttpPost("Approve")]
        public async Task<IActionResult> Approve([FromBody] MembershipApprovalRequest request)
        {
            if (!IsSuperAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này" });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(request.MembershipId))
                {
                    return Json(new { success = false, message = "ID thành viên không hợp lệ" });
                }

                var adminId = GetCurrentUserId();
                await _membershipApprovalService.ApproveMembershipAsync(request.MembershipId, adminId);
                return Json(new { success = true, message = "Phê duyệt thành viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving membership");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Từ chối thành viên
        /// </summary>
        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] MembershipApprovalRequest request)
        {
            if (!IsSuperAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này" });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(request.MembershipId))
                {
                    return Json(new { success = false, message = "ID thành viên không hợp lệ" });
                }

                if (string.IsNullOrWhiteSpace(request.ApprovalReason))
                {
                    return Json(new { success = false, message = "Lý do từ chối không được để trống" });
                }

                var adminId = GetCurrentUserId();
                await _membershipApprovalService.RejectMembershipAsync(request.MembershipId, request.ApprovalReason, adminId);
                return Json(new { success = true, message = "Từ chối thành viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting membership");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Trang danh sách thành viên đã phê duyệt
        /// </summary>
        [HttpGet("Approved")]
        public async Task<IActionResult> Approved(int page = 1, int pageSize = 20, string? keyword = null)
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                var result = await _membershipApprovalService.GetApprovedMembershipsAsync(page, pageSize, keyword);
                ViewBag.Keyword = keyword;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = result.TotalPages;
                return View(result.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading approved memberships");
                return View(new List<Membership>());
            }
        }

        /// <summary>
        /// Trang danh sách thành viên bị từ chối
        /// </summary>
        [HttpGet("Rejected")]
        public async Task<IActionResult> Rejected(int page = 1, int pageSize = 20, string? keyword = null)
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                var result = await _membershipApprovalService.GetRejectedMembershipsAsync(page, pageSize, keyword);
                ViewBag.Keyword = keyword;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = result.TotalPages;
                return View(result.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rejected memberships");
                return View(new List<Membership>());
            }
        }
    }
}

