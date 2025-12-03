using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Services.Refs;
using MiniAppGIBA.Models.Queries.Refs;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("Refs")]
    public class RefsController : BaseCMSController
    {
        private readonly IRefService _refService;
        private readonly ILogger<RefsController> _logger;

        public RefsController(
            IRefService refService,
            ILogger<RefsController> logger)
        {
            _refService = refService;
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

        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage(
            int page = 1,
            int pageSize = 10,
            string? keyword = null,
            byte? status = null,
            byte? type = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            byte? minRating = null,
            byte? maxRating = null,
            string? sortBy = "date",
            string? sortOrder = "desc")
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var queryParameters = new RefQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Keyword = keyword,
                    Status = status,
                    Type = type,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MinRating = minRating,
                    MaxRating = maxRating,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                // Get allowed group IDs for ADMIN users (null for SUPER_ADMIN)
                var allowedGroupIds = GetUserGroupIdsOrNull();
                var result = await _refService.GetRefsForCMSAsync(queryParameters, allowedGroupIds);

                var responseData = result.Items.Select(r => new
                {
                    r.Id,
                    r.RefFrom,
                    r.RefTo,
                    r.Content,
                    r.Status,
                    r.StatusText,
                    r.Type,
                    r.TypeText,
                    r.Value,
                    r.RefToGroupId,
                    r.RefToGroupName,
                    r.ReferredMemberGroupId,
                    r.ReferredMemberGroupName,
                    r.Rating,
                    r.Feedback,
                    r.RatingDate,
                    r.CreatedDate,
                    r.UpdatedDate,
                    FromMemberName = r.FromMemberName ?? "N/A",
                    FromMemberCompany = r.FromMemberCompany ?? "N/A",
                    FromMemberPosition = r.FromMemberPosition ?? "N/A",
                    ToMemberName = r.ToMemberName ?? "N/A",
                    ToMemberCompany = r.ToMemberCompany ?? "N/A",
                    ToMemberPosition = r.ToMemberPosition ?? "N/A",
                    ToMemberPhone = r.ToMemberPhone,
                    ToMemberAvatar = r.ToMemberAvatar,
                    ToMemberSlug = r.ToMemberSlug,
                    FromMemberPhone = r.FromMemberPhone,
                    FromMemberAvatar = r.FromMemberAvatar,
                    FromMemberSlug = r.FromMemberSlug,
                    // Type 0 fields
                    ReferralName = r.ReferralName,
                    ReferralPhone = r.ReferralPhone,
                    ReferralEmail = r.ReferralEmail,
                    ReferralAddress = r.ReferralAddress,
                    // Type 1 fields
                    RecipientName = r.RecipientName,
                    RecipientPhone = r.RecipientPhone,
                    
                    ReferredMemberId = r.ReferredMemberId,
                    ReferredMemberName = r.ReferredMemberName,
                    ReferredMemberCompany = r.ReferredMemberCompany,
                    ReferredMemberPosition = r.ReferredMemberPosition,
                    ReferredMemberPhone = r.ReferredMemberPhone,
                    ReferredMemberEmail = r.ReferredMemberEmail,
                    ReferredMemberAvatar = r.ReferredMemberAvatar,
                    ReferredMemberSlug = r.ReferredMemberSlug,
                    // ReferredMemberSnapshot = r.ReferredMemberSnapshot,
                    RatingDisplay = r.Rating.HasValue ? $"{r.Rating}/5" : "Chưa đánh giá",
                    CreatedDateDisplay = r.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    UpdatedDateDisplay = r.UpdatedDate.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = responseData,
                    pagination = new
                    {
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalItems = result.TotalItems,
                        totalPages = result.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs for CMS");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var ref_data = await _refService.GetRefByIdAsync(id);
                if (ref_data == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ref" });
                }

                // Check group permission
                if (!IsSuperAdmin())
                {
                    var allowedGroupIds = GetUserGroupIds();
                    var userGroupIds = await _refService.GetUserGroupIdsAsync(ref_data.RefFrom ?? "");

                    if (!userGroupIds.Any(gid => allowedGroupIds.Contains(gid)))
                    {
                        return Json(new { success = false, message = "Không có quyền xem ref này" });
                    }
                }

                return Json(new { success = true, data = ref_data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ref {RefId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

