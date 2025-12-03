using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Services.Events;
using MiniAppGIBA.Services.EventRegistrations;
using MiniAppGIBA.Services.EventGuests;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Enum;
namespace MiniAppGIBA.Controller.CMS
{
    [Route("EventRegistrations")]
    public class EventRegistrationsController : BaseCMSController
    {
        private readonly IEventService _eventService;
        private readonly IEventRegistrationService _eventRegistrationService;
        private readonly IEventGuestService _eventGuestService;
        private readonly ILogger<EventRegistrationsController> _logger;

        public EventRegistrationsController(
            IEventService eventService,
            IEventRegistrationService eventRegistrationService,
            IEventGuestService eventGuestService,
            ILogger<EventRegistrationsController> logger)
        {
            _eventService = eventService;
            _eventRegistrationService = eventRegistrationService;
            _eventGuestService = eventGuestService;
            _logger = logger;
        }

        [HttpGet("CheckIn/{eventId}")]
        public async Task<IActionResult> CheckIn(string eventId)
        {
            try
            {
                var eventDetail = await _eventService.GetEventByIdAsync(eventId);
                if (eventDetail == null)
                {
                    return NotFound("Không tìm thấy sự kiện");
                }

                ViewBag.Banner = eventDetail.Banner;
                return View(eventDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading check-in page for event {EventId}", eventId);
                return BadRequest("Có lỗi xảy ra khi tải trang check-in");
            }
        }

        [HttpGet("GetCapacityInfo/{eventId}")]
        public async Task<IActionResult> GetCapacityInfo(string eventId)
        {
            try
            {
                var capacityInfo = await _eventService.GetEventCapacityInfoAsync(eventId);
                return Json(capacityInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting capacity info for event {EventId}", eventId);
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet("GetAll/{eventId}")]
        public async Task<IActionResult> GetAll(string eventId, [FromQuery] int page = 1, [FromQuery] int pagesize = 10, [FromQuery] string? keyword = null)
        {
            try
            {
                _logger.LogInformation("Getting event registrations for event {EventId}, page {Page}, pageSize {PageSize}", eventId, page, pagesize);

                var result = await _eventRegistrationService.GetEventRegistrationsAsync(eventId, page, pagesize, keyword);

                _logger.LogInformation("Found {Count} registrations for event {EventId}", result.Items.Count, eventId);

                var response = new
                {
                    data = result.Items.Select(item => new
                    {
                        name = item.Name,
                        phoneNumber = item.Phone,
                        email = item.Email,
                        checkInCode = item.CheckInCode,
                        status = item.Status,
                        statusText = item.StatusText
                    }),
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = page
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event registrations for event {EventId}", eventId);
                return Json(new { data = new List<object>(), totalItems = 0, totalPages = 0, currentPage = page });
            }
        }

        [HttpGet("GetAllParticipants/{eventId}")]
        public async Task<IActionResult> GetAllParticipants(string eventId, [FromQuery] int page = 1, [FromQuery] int pagesize = 10, [FromQuery] string? keyword = null)
        {
            try
            {
                _logger.LogInformation("Getting all participants for event {EventId}, page {Page}, pageSize {PageSize}", eventId, page, pagesize);

                var registrations = await _eventRegistrationService.GetEventRegistrationsAsync(eventId, page, pagesize, keyword);

                var approvedGuests = await _eventGuestService.GetEventGuestListsAsync(eventId);
                var approvedGuestList = approvedGuests.Where(g => g.Status == 1).ToList();

                var allParticipants = new List<object>();

                foreach (var reg in registrations.Items)
                {
                        if (reg.Status != 1 && reg.Status !=2) {
                            continue;
                        } 
                    allParticipants.Add(new
                    {
                        name = reg.Name,
                        phoneNumber = reg.Phone,
                        email = reg.Email,
                        checkInCode = reg.CheckInCode,
                        status = reg.Status,
                        statusText = reg.StatusText,
                        type = "Đăng ký trực tiếp"
                    });
                }

                // Thêm khách mời đã duyệt
                foreach (var guest in approvedGuestList)
                {
                    foreach (var guestDetail in guest.GuestLists)
                    {
                        // Chỉ hiển thị khách mời đã được duyệt
                        if (guestDetail.Status != (byte)EGuestStatus.Approved) {
                            continue;
                        }
                        allParticipants.Add(new
                        {
                            name = guestDetail.GuestName,
                            phoneNumber = guestDetail.GuestPhone,
                            email = guestDetail.GuestEmail,
                            checkInCode = guestDetail.CheckInCode,
                            status = guestDetail.Status,
                            statusText = guestDetail.StatusText,
                            checkInStatus = guestDetail.CheckInStatus,
                            type = "Khách mời"
                        });
                    }
                }

                var response = new
                {
                    data = allParticipants,
                    totalItems = allParticipants.Count,
                    totalPages = (int)Math.Ceiling((double)allParticipants.Count / pagesize),
                    currentPage = page
                };

                _logger.LogInformation("Found {Count} total participants for event {EventId}", allParticipants.Count, eventId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all participants for event {EventId}", eventId);
                return Json(new { data = new List<object>(), totalItems = 0, totalPages = 0, currentPage = page });
            }
        }

        [HttpGet("GetStatistics/{eventId}")]
        public async Task<IActionResult> GetStatistics(string eventId)
        {
            try
            {
                var statistics = await _eventService.GetEventStatisticsAsync(eventId);
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event statistics for event {EventId}", eventId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê sự kiện" });
            }
        }
    }
}
