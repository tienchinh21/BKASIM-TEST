using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Sponsors;
using MiniAppGIBA.Models.Request.Sponsors;
using MiniAppGIBA.Services.Sponsors;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("Sponsor")]
    public class SponsorController : BaseCMSController
    {
        private readonly ISponsorService _sponsorService;
        private readonly ILogger<SponsorController> _logger;

        public SponsorController(ISponsorService sponsorService, ILogger<SponsorController> logger)
        {
            _sponsorService = sponsorService;
            _logger = logger;
        }

        /// <summary>
        /// Trang chính quản lý nhà tài trợ
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
        /// Lấy dữ liệu nhà tài trợ cho DataTable
        /// </summary>
        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] SponsorQueryParameters query)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }
            
            try
            {
                var result = await _sponsorService.GetSponsorsAsync(query);
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
                _logger.LogError(ex, "Error getting sponsors page data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu", draw = query.Draw, recordsTotal = 0, recordsFiltered = 0, data = new List<object>() });
            }
        }

        /// <summary>
        /// Hiển thị form tạo nhà tài trợ mới
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền tạo nhà tài trợ!" });
            }
            
            try
            {
                ViewBag.IsEdit = false;
                ViewBag.Title = "Tạo Nhà Tài Trợ Mới";
                ViewBag.Button = "Tạo Nhà Tài Trợ";

                var model = new CreateSponsorRequest();
                return PartialView("Partials/_SponsorForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create sponsor form");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form tạo nhà tài trợ" });
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa nhà tài trợ
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền chỉnh sửa nhà tài trợ!" });
            }
            
            try
            {
                var sponsor = await _sponsorService.GetSponsorByIdAsync(id);
                if (sponsor == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà tài trợ" });
                }

                ViewBag.IsEdit = true;
                ViewBag.SponsorId = id;
                ViewBag.Title = "Chỉnh Sửa Nhà Tài Trợ";
                ViewBag.Button = "Cập Nhật";
                ViewBag.Image = sponsor.Image;

                var model = new UpdateSponsorRequest
                {
                    Id = sponsor.Id,
                    SponsorName = sponsor.SponsorName ?? string.Empty,
                    Introduction = sponsor.Introduction,
                    WebsiteURL = sponsor.WebsiteURL,
                    IsActive = sponsor.IsActive
                };

                return PartialView("Partials/_SponsorForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit sponsor form for {SponsorId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form chỉnh sửa nhà tài trợ" });
            }
        }

        /// <summary>
        /// Tạo nhà tài trợ mới
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateSponsorRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền tạo nhà tài trợ!" });
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

                var result = await _sponsorService.CreateSponsorAsync(request);
                return Json(new { success = true, message = "Tạo nhà tài trợ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsor");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo nhà tài trợ" });
            }
        }

        /// <summary>
        /// Cập nhật nhà tài trợ
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UpdateSponsorRequest request)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền chỉnh sửa nhà tài trợ!" });
            }
            
            try
            {
                _logger.LogInformation("🔄 CONTROLLER: Received EDIT request for Sponsor ID: '{Id}' with ShouldRemoveImage: {ShouldRemoveImage}", 
                    request.Id ?? "null", request.ShouldRemoveImage);
                _logger.LogInformation("🔄 REQUEST DETAILS: SponsorName='{SponsorName}', IsActive={IsActive}", 
                    request.SponsorName, request.IsActive);
                    
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

                var result = await _sponsorService.UpdateSponsorAsync(request.Id!, request);
                return Json(new { success = true, message = "Cập nhật nhà tài trợ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsor {SponsorId}", request.Id ?? "null");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật nhà tài trợ" });
            }
        }

        /// <summary>
        /// Xóa nhà tài trợ
        /// </summary>
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Chỉ Admin mới có quyền xóa nhà tài trợ!" });
            }
            
            try
            {
                var result = await _sponsorService.DeleteSponsorAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa nhà tài trợ thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà tài trợ" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sponsor {SponsorId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa nhà tài trợ" });
            }
        }
    }
}
