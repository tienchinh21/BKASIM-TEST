using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.Events;
using MiniAppGIBA.Service.Groups;
using MiniAppGIBA.Services.Sponsors;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Models.Queries.Groups;

namespace MiniAppGIBA.Controller.CMS
{
    
    [Route("Event")]
    public class EventController : BaseCMSController
    {
        private readonly IEventService _eventService;
        private readonly IGroupService _groupService;
        private readonly ISponsorService _sponsorService;
        private readonly ISponsorshipTierService _sponsorshipTierService;
        private readonly IEventCustomFieldService _eventCustomFieldService;
        private readonly ILogger<EventController> _logger;

        public EventController(
            IEventService eventService,
            IGroupService groupService,
            ISponsorService sponsorService,
            ISponsorshipTierService sponsorshipTierService,
            IEventCustomFieldService eventCustomFieldService,
            ILogger<EventController> logger)
        {
            _eventService = eventService;
            _groupService = groupService;
            _sponsorService = sponsorService;
            _sponsorshipTierService = sponsorshipTierService;
            _eventCustomFieldService = eventCustomFieldService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách sự kiện
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                // GIBA has full access to all groups - no filtering needed
                var groups = await _groupService.GetActiveGroupsAsync();
                ViewBag.Groups = groups;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events index page");
                return View("Error");
            }
        }

        /// <summary>
        /// Lấy dữ liệu phân trang cho DataTable
        /// </summary>
        [HttpGet("GetPage")]
        [Authorize]
        public async Task<IActionResult> GetPage([FromQuery] EventQueryParameters query)
        {
            try
            {
                // Filter by allowed group IDs (for ADMIN users)
                string? roleName = User.GetRoles().FirstOrDefault();
                var userId = GetCurrentUserId();
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                var result = await _eventService.GetEventsAsync(query, roleName, userId, userZaloId);

                var response = new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = result.TotalItems,
                    recordsFiltered = result.TotalItems,
                    data = result.Items.Select(item => new
                    {
                        id = item.Id,
                        title = item.Title,
                        groupName = item.GroupName,
                        startTime = item.StartTime.ToString("dd/MM/yyyy HH:mm"),
                        endTime = item.EndTime.ToString("dd/MM/yyyy HH:mm"),
                        type = item.Type,
                        typeText = item.TypeText,
                        status = item.Status,
                        statusText = item.StatusText,
                        statusClass = item.StatusClass,
                        joinCount = item.JoinCount,
                        isActive = item.IsActive,
                        createdDate = item.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                        address = item.Address,
                        meetingLink = item.MeetingLink,
                        googleMapURL = item.GoogleMapURL,
                        banner = item.Banner,
                        images = item.Images,
                        isRegister = item.IsRegister,
                        isCheckIn = item.IsCheckIn,
                        checkInCode = item.CheckInCode,
                        needApproval = item.NeedApproval
                    })
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events page data");
                return Json(new { error = "Có lỗi xảy ra khi tải dữ liệu" });
            }
        }

        /// <summary>
        /// Hiển thị form tạo sự kiện
        /// </summary>
        [HttpGet("Create")]
        [Authorize]
        public async Task<IActionResult> GetCreateForm()
        {
            try
            {
                // Check if user is GIBA admin
                if (!IsAdmin())
                {
                    return Json(new { success = false, message = "Không tìm thấy quyền của người dùng" });
                }

                // GIBA has full access - load all active groups
                var query = new GroupQueryParameters
                {
                    PageSize = int.MaxValue,
                    Page = 1,
                    Status = true // Chỉ lấy groups active
                };
                
                var result = await _groupService.GetGroupsAsync(query);
                ViewBag.Groups = result.Items;
                ViewBag.IsEdit = false;
                ViewBag.Title = "Tạo Sự Kiện Mới";
                ViewBag.Button = "Tạo Sự Kiện";

                var model = new CreateEventRequest();
                return PartialView("Partials/_EventForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create event form");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form tạo sự kiện" });
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa sự kiện
        /// </summary>
        [HttpGet("Edit/{id}")]
        [Authorize]
        public async Task<IActionResult> GetEditForm(string id)
        {
            try
            {
                var eventDetail = await _eventService.GetEventByIdAsync(id);
                if (eventDetail == null)
                {
                    return NotFound();
                }

                // Check if ADMIN has permission to edit this event
                // if (!IsSuperAdmin() && !HasGroupPermission(eventDetail.GroupId))
                // {
                //     return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa sự kiện này!" });
                // }

                // GIBA has full access - load all active groups
                var groups = await _groupService.GetActiveGroupsAsync();
                ViewBag.Groups = groups;
                ViewBag.IsEdit = true;
                ViewBag.EventId = id;
                ViewBag.Title = "Chỉnh Sửa Sự Kiện";
                ViewBag.Button = "Cập Nhật";

                // Convert to UpdateEventRequest for form binding
                var model = new UpdateEventRequest
                {
                    Id = eventDetail.Id,
                    GroupId = eventDetail.GroupId,
                    Title = eventDetail.Title,
                    Content = eventDetail.Content,
                    StartTime = eventDetail.StartTime,
                    EndTime = eventDetail.EndTime,
                    Type = eventDetail.Type,
                    JoinCount = eventDetail.JoinCount,
                    MeetingLink = eventDetail.MeetingLink,
                    GoogleMapURL = eventDetail.GoogleMapURL,
                    Address = eventDetail.Address,
                    IsActive = eventDetail.IsActive,
                    NeedApproval = eventDetail.NeedApproval
                };

                // Pass images and banner for display
                ViewBag.Images = eventDetail.Images;
                ViewBag.Banner = eventDetail.Banner;

                // Pass gift data for display
                ViewBag.EventGift = eventDetail.Gifts?.FirstOrDefault();

                // Debug: Log gift information
                Console.WriteLine($"Event {id} - Gifts count: {eventDetail.Gifts?.Count ?? 0}");
                if (eventDetail.Gifts?.Any() == true)
                {
                    var firstGift = eventDetail.Gifts.First();
                    Console.WriteLine($"First gift - ID: {firstGift.Id}, Name: {firstGift.GiftName}, Images: {firstGift.Images?.Count ?? 0}");
                    if (firstGift.Images?.Any() == true)
                    {
                        Console.WriteLine($"First image: {firstGift.Images.First()}");
                    }
                }

                // Pass sponsor data for display
                var eventSponsor = eventDetail.Sponsors?.FirstOrDefault();
                ViewBag.CurrentSponsorId = eventSponsor?.SponsorId;
                ViewBag.CurrentSponsorshipTierId = eventSponsor?.SponsorshipTierId;

                return PartialView("Partials/_EventForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit event form for {EventId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form chỉnh sửa sự kiện" });
            }
        }

        /// <summary>
        /// Tạo sự kiện mới
        /// </summary>
        [HttpPost("Create")]
        [Authorize] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateEventRequest request)
        {
            try
            {
                // Check if ADMIN has permission to create event for this group
                // if (!IsSuperAdmin() && !HasGroupPermission(request.GroupId))
                // {
                //     return Json(new { success = false, message = "Bạn không có quyền tạo sự kiện cho nhóm này!" });
                // }

                _logger.LogInformation("Creating event with data: {@Request}", new
                {
                    request.GroupId,
                    request.Title,
                    request.StartTime,
                    request.EndTime,
                    request.Type,
                    request.JoinCount,
                    request.Content?.Length,
                    BannerFileSize = request.BannerFile?.Length,
                    HasImageFile = request.ImageFiles != null,
                    request.GiftsData
                });
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value?.Errors.Select(e => e.ErrorMessage ?? string.Empty) ?? Enumerable.Empty<string>()
                        })
                        .ToList();

                    _logger.LogWarning("ModelState validation failed: {@Errors}", errors);

                    var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                    return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {errorMessage}" });
                }

                var result = await _eventService.CreateEventAsync(request);
                return Json(new { success = true, message = "Tạo sự kiện thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo sự kiện" });
            }
        }

        /// <summary>
        /// Cập nhật sự kiện
        /// </summary>
        [HttpPost("Edit")]
        [Authorize]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> Edit([FromForm] UpdateEventRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                // Get event to check group permission
                var eventDetail = await _eventService.GetEventByIdAsync(request.Id);
                if (eventDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sự kiện" });
                }

                // Check if ADMIN has permission to edit this event
                // if (!IsSuperAdmin() && !HasGroupPermission(eventDetail.GroupId))
                // {
                //     return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa sự kiện này!" });
                // }

                var result = await _eventService.UpdateEventAsync(request.Id, request);
                return Json(new { success = true, message = "Cập nhật sự kiện thành công", data = result });
            }
            catch (KeyNotFoundException)
            {
                return Json(new { success = false, message = "Không tìm thấy sự kiện" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", request.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật sự kiện" });
            }
        }

        /// <summary>
        /// Xóa sự kiện
        /// </summary>
        [HttpPost("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _eventService.DeleteEventAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy sự kiện" });
                }

                return Json(new { success = true, message = "Xóa sự kiện thành công" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sự kiện" });
            }
        }

        /// <summary>
        /// Thay đổi trạng thái sự kiện
        /// </summary>
        [HttpPost("ToggleStatus")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var result = await _eventService.ToggleEventStatusAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy sự kiện" });
                }

                return Json(new { success = true, message = "Thay đổi trạng thái sự kiện thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling event status {EventId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái sự kiện" });
            }
        }

        /// <summary>
        /// Lấy danh sách nhà tài trợ hoạt động
        /// </summary>
        [HttpGet("GetActiveSponsors")]
        [Authorize]
        public async Task<IActionResult> GetActiveSponsors()
        {
            try
            {
                var sponsors = await _sponsorService.GetActiveSponsorsAsync();
                return Json(new { success = true, data = sponsors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sponsors");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách nhà tài trợ." });
            }
        }

        /// <summary>
        /// Lấy danh sách hạng tài trợ hoạt động
        /// </summary>
        [HttpGet("GetActiveSponsorshipTiers")]
        [Authorize]
        public async Task<IActionResult> GetActiveSponsorshipTiers()
        {
            try
            {
                var tiers = await _sponsorshipTierService.GetActiveSponsorshipTiersAsync();
                return Json(new { success = true, data = tiers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sponsorship tiers");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách hạng tài trợ." });
            }
        }

        [HttpGet("Statistics/{id}")]
        [Authorize]
        public async Task<IActionResult> Statistics(string id)
        {
            try
            {
                var statistics = await _eventService.GetEventStatisticsAsync(id);
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event statistics for event {EventId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy thống kê sự kiện.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("ExportStatistics/{id}")]
        [Authorize]
        public async Task<IActionResult> ExportStatistics(string id)
        {
            try
            {
                var excelData = await _eventService.ExportEventStatisticsAsync(id);
                var statistics = await _eventService.GetEventStatisticsAsync(id);
                var fileName = $"BaoCaoSuKien_{statistics.EventTitle}_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting event statistics for event {EventId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo.";
                return RedirectToAction("Statistics", new { id });
            }
        }

        /// Lấy danh sách trường tùy chỉnh của sự kiện
        [HttpGet("GetCustomFields/{eventId}")]
        public async Task<IActionResult> GetCustomFields(string eventId)
        {
            try
            {
                if (string.IsNullOrEmpty(eventId))
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                var customFields = await _eventCustomFieldService.GetEventCustomFieldsAsync(eventId);
                
                return Json(new 
                { 
                    success = true, 
                    data = customFields ?? new List<MiniAppGIBA.Models.DTOs.Events.EventCustomFieldDTO>() 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields for event {EventId}", eventId);
                return Json(new { success = true, data = new List<object>() });
            }
        }
    }
}