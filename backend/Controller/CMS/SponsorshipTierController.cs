using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Sponsors;
using MiniAppGIBA.Models.Request.Sponsors;
using MiniAppGIBA.Services.Sponsors;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("SponsorshipTier")]
    public class SponsorshipTierController : BaseCMSController
    {
        private readonly ISponsorshipTierService _sponsorshipTierService;
        private readonly ILogger<SponsorshipTierController> _logger;

        public SponsorshipTierController(ISponsorshipTierService sponsorshipTierService, ILogger<SponsorshipTierController> logger)
        {
            _sponsorshipTierService = sponsorshipTierService;
            _logger = logger;
        }

        /// <summary>
        /// Trang chính quản lý hạng nhà tài trợ
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            return View();
        }

        /// <summary>
        /// Lấy dữ liệu hạng nhà tài trợ cho DataTable
        /// </summary>
        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] SponsorshipTierQueryParameters query)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }
            
            try
            {
                var result = await _sponsorshipTierService.GetSponsorshipTiersAsync(query);
                return Json(new
                {
                    draw = query.Draw,
                    recordsTotal = result.TotalItems,
                    recordsFiltered = result.TotalItems,
                    data = result.Items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsorship tiers page data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu", draw = query.Draw, recordsTotal = 0, recordsFiltered = 0, data = new List<object>() });
            }
        }

        /// <summary>
        /// Hiển thị form tạo hạng nhà tài trợ mới
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền tạo hạng nhà tài trợ!" });
            }
            
            try
            {
                ViewBag.IsEdit = false;
                ViewBag.Title = "Tạo Hạng Nhà Tài Trợ Mới";
                ViewBag.Button = "Tạo Hạng Tài Trợ";

                var model = new CreateSponsorshipTierRequest();
                return PartialView("Partials/_SponsorshipTierForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create sponsorship tier form");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form tạo hạng nhà tài trợ" });
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa hạng nhà tài trợ
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền chỉnh sửa hạng nhà tài trợ!" });
            }
            
            try
            {
                var sponsorshipTier = await _sponsorshipTierService.GetSponsorshipTierByIdAsync(id);
                if (sponsorshipTier == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hạng nhà tài trợ" });
                }

                ViewBag.IsEdit = true;
                ViewBag.SponsorshipTierId = id;
                ViewBag.Title = "Chỉnh Sửa Hạng Nhà Tài Trợ";
                ViewBag.Button = "Cập Nhật";
                ViewBag.Image = sponsorshipTier.Image;

                var model = new UpdateSponsorshipTierRequest
                {
                    Id = sponsorshipTier.Id,
                    TierName = sponsorshipTier.TierName ?? string.Empty,
                    Description = sponsorshipTier.Description,
                    IsActive = sponsorshipTier.IsActive
                };

                return PartialView("Partials/_SponsorshipTierForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit sponsorship tier form for {SponsorshipTierId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form chỉnh sửa hạng nhà tài trợ" });
            }
        }

        /// <summary>
        /// Tạo hạng nhà tài trợ mới
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateSponsorshipTierRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền tạo hạng nhà tài trợ!" });
            }
            
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()
                        })
                        .ToList();

                    var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                    return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {errorMessage}" });
                }

                var result = await _sponsorshipTierService.CreateSponsorshipTierAsync(request);
                return Json(new { success = true, message = "Tạo hạng nhà tài trợ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsorship tier");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo hạng nhà tài trợ" });
            }
        }

        /// <summary>
        /// Cập nhật hạng nhà tài trợ
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UpdateSponsorshipTierRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền chỉnh sửa hạng nhà tài trợ!" });
            }
            
            try
            {
                _logger.LogInformation("Received edit request for ID: {Id} with ShouldRemoveImage: {ShouldRemoveImage}", 
                    request.Id ?? "null", request.ShouldRemoveImage);
                    
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()
                        })
                        .ToList();

                    var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                    _logger.LogWarning("Model validation failed: {ErrorMessage}", errorMessage);
                    return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {errorMessage}" });
                }

                var result = await _sponsorshipTierService.UpdateSponsorshipTierAsync(request.Id!, request);
                return Json(new { success = true, message = "Cập nhật hạng nhà tài trợ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsorship tier {SponsorshipTierId}", request.Id ?? "null");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hạng nhà tài trợ" });
            }
        }

        /// <summary>
        /// Xóa hạng nhà tài trợ
        /// </summary>
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền xóa hạng nhà tài trợ!" });
            }
            
            try
            {
                var result = await _sponsorshipTierService.DeleteSponsorshipTierAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa hạng nhà tài trợ thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy hạng nhà tài trợ" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sponsorship tier {SponsorshipTierId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hạng nhà tài trợ" });
            }
        }
    }
}
