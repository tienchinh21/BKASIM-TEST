using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Request.Groups;
using MiniAppGIBA.Services.Groups;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Service.Groups;
using MiniAppGIBA.Services.Subscriptions;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using MiniAppGIBA.Services.Memberships;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("Membership")]
    public class PendingApprovalController : BaseCMSController
    {
        private readonly IMembershipGroupService _membershipGroupService;
        private readonly IMemberSubscriptionService _memberSubscriptionService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly IMembershipService _membershipService;
        private readonly ILogger<PendingApprovalController> _logger;

        public PendingApprovalController(
            IMembershipGroupService membershipGroupService,
            IMemberSubscriptionService memberSubscriptionService,
            ISubscriptionPlanService subscriptionPlanService,
            IMembershipService membershipService,
            ILogger<PendingApprovalController> logger)
        {
            _membershipGroupService = membershipGroupService;
            _memberSubscriptionService = memberSubscriptionService;
            _subscriptionPlanService = subscriptionPlanService;
            _membershipService = membershipService;
            _logger = logger;
        }

        [HttpGet("PendingApproval")]
        public IActionResult PendingApproval()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View("~/Views/Membership/PendingApproval.cshtml");
        }

        [HttpGet("ViewDetail/{id}")]
        public async Task<IActionResult> ViewDetail(string id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            try
            {
                _logger.LogInformation("ViewDetail called with id: {Id}", id);

                var membershipGroup = await _membershipGroupService.GetMembershipGroupByIdAsync(id);
                if (membershipGroup == null)
                {
                    _logger.LogWarning("MembershipGroup not found for id: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("Found MembershipGroup with UserZaloId: {UserZaloId}", membershipGroup.UserZaloId);

                // Get full membership details using UserZaloId
                var fullMembership = await _membershipService.GetMembershipByUserZaloIdAsync(membershipGroup.UserZaloId);
                
                _logger.LogInformation("FullMembership result: {FullMembership}", fullMembership != null ? "Found" : "Not found");

                ViewBag.MembershipGroupId = membershipGroup.Id;
                ViewBag.IsApproved = membershipGroup.IsApproved;
                ViewBag.FullMembership = fullMembership; // Pass full membership data

                return PartialView("~/Views/Membership/Partials/_ApprovalForm.cshtml", membershipGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewDetail for id: {Id}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("GetPendingPage")]
        public async Task<IActionResult> GetPendingPage(
            int page = 1,
            int pageSize = 10,
            string? keyword = null,
            string? groupId = null,
            string? status = null)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var queryParameters = new PendingApprovalQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Keyword = keyword,
                    GroupId = groupId
                };

                // Parse status filter - default to "all" if not specified
                if (string.IsNullOrEmpty(status))
                {
                    status = "all"; // Default to show all statuses
                }

                if (status == "all")
                {
                    queryParameters.ShouldFilterByApprovalStatus = false; // Get all statuses
                }
                else if (status == "pending")
                {
                    queryParameters.IsApproved = null;
                    queryParameters.ShouldFilterByApprovalStatus = true;
                }
                else if (bool.TryParse(status, out bool isApproved))
                {
                    queryParameters.IsApproved = isApproved;
                    queryParameters.ShouldFilterByApprovalStatus = true;
                }

                // Filter by allowed group IDs (for ADMIN users)
                var allowedGroupIds = GetUserGroupIdsOrNull();
                var result = await _membershipGroupService.GetMembershipGroupsAsync(queryParameters, allowedGroupIds);

                var responseData = result.Items.Select(mg => new
                {
                    id = mg.Id,
                    memberName = mg.MemberName,
                    groupName = mg.GroupName,
                    company = mg.Company,
                    position = mg.Position,
                    reason = mg.Reason,
                    isApproved = mg.IsApproved,
                    statusText = mg.StatusText,
                    statusClass = mg.StatusClass,
                    rejectReason = mg.RejectReason,
                    approvedDate = mg.ApprovedDate?.ToString("dd/MM/yyyy HH:mm"),
                    createdDate = mg.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    // Store raw data for detail view
                    rawData = new
                    {
                        userZaloId = mg.UserZaloId,
                        ZaloAvatar = mg.ZaloAvatar,
                        groupId = mg.GroupId,
                        company = mg.Company,
                        position = mg.Position,
                        reason = mg.Reason,
                        isApproved = mg.IsApproved,
                        rejectReason = mg.RejectReason
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
                _logger.LogError(ex, "Error getting pending approvals");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve([FromForm] string id, [FromForm] string? subscriptionPlanId = null, [FromForm] int? additionalDays = null, [FromForm] DateTime? customStartDate = null)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Get membership group to check permission
                var membershipGroup = await _membershipGroupService.GetMembershipGroupByIdAsync(id);
                if (membershipGroup == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn xin tham gia" });
                }

                // Check if ADMIN has permission to approve for this group
                if (!IsSuperAdmin() && !HasGroupPermission(membershipGroup.GroupId))
                {
                    return Json(new { success = false, message = "Bạn không có quyền phê duyệt đơn này!" });
                }

                var request = new ApproveRejectRequest
                {
                    Id = id,
                    IsApproved = true,
                    SubscriptionPlanId = subscriptionPlanId,
                    AdditionalDays = additionalDays,
                    CustomStartDate = customStartDate
                };

                await _membershipGroupService.ApproveOrRejectAsync(request);

                // If subscription plan is provided, create member subscription
                if (!string.IsNullOrEmpty(subscriptionPlanId))
                {
                    try
                    {
                        var createSubscriptionRequest = new CreateMemberSubscriptionDTO
                        {
                            MembershipGroupId = id,
                            SubscriptionPlanId = subscriptionPlanId,
                            StartDate = customStartDate ?? DateTime.Now,
                            IsActive = true,
                            Notes = "Tự động tạo khi duyệt thành viên"
                        };

                        // Add additional days if specified
                        if (additionalDays.HasValue && additionalDays > 0)
                        {
                            createSubscriptionRequest.StartDate = createSubscriptionRequest.StartDate.AddDays(additionalDays.Value);
                        }

                        await _memberSubscriptionService.CreateAsync(createSubscriptionRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create subscription for member {MembershipGroupId}", id);
                        // Continue with approval even if subscription creation fails
                    }
                }

                return Json(new { success = true, message = "Phê duyệt thành công!" });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving membership group {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi phê duyệt!" });
            }
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject([FromForm] string id, [FromForm] string rejectReason)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Get membership group to check permission
                var membershipGroup = await _membershipGroupService.GetMembershipGroupByIdAsync(id);
                if (membershipGroup == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn xin tham gia" });
                }

                // Check if ADMIN has permission to reject for this group
                if (!IsSuperAdmin() && !HasGroupPermission(membershipGroup.GroupId))
                {
                    return Json(new { success = false, message = "Bạn không có quyền từ chối đơn này!" });
                }

                var request = new ApproveRejectRequest
                {
                    Id = id,
                    IsApproved = false,
                    RejectReason = rejectReason
                };

                await _membershipGroupService.ApproveOrRejectAsync(request);
                return Json(new { success = true, message = "Từ chối thành công!" });
            }
            catch (CustomException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting membership group {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi từ chối!" });
            }
        }

        [HttpGet("GetPendingCount")]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var count = await _membershipGroupService.GetPendingCountAsync();
                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending count");
                return Json(new { success = false, count = 0 });
            }
        }

        /// <summary>
        /// API lấy danh sách gói cước đang hoạt động
        /// </summary>
        [HttpGet("GetActivePlans")]
        public async Task<IActionResult> GetActivePlans()
        {
            try
            {
                var plans = await _subscriptionPlanService.GetActivePlansAsync();
                return Json(new { success = true, data = plans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscription plans");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// API lấy danh sách gói cước theo GroupId
        /// </summary>
        [HttpGet("GetPlansByGroup/{groupId}")]
        public async Task<IActionResult> GetPlansByGroup(string groupId)
        {
            try
            {
                var plans = await _subscriptionPlanService.GetPlansByGroupIdAsync(groupId);
                return Json(new { success = true, data = plans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription plans by group");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách gói cước" });
            }
        }

        /// <summary>
        /// Test API to debug membership service
        /// </summary>
        [HttpGet("TestMembership/{userZaloId}")]
        public async Task<IActionResult> TestMembership(string userZaloId)
        {
            try
            {
                _logger.LogInformation("Testing membership service with UserZaloId: {UserZaloId}", userZaloId);
                
                var membership = await _membershipService.GetMembershipByUserZaloIdAsync(userZaloId);
                
                if (membership == null)
                {
                    return Json(new { success = false, message = "Membership not found" });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        membership.Id,
                        membership.Fullname,
                        membership.PhoneNumber,
                        membership.ZaloAvatar,
                        membership.RoleId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing membership service");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

