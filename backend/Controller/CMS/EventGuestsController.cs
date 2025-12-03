using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.EventGuests;

namespace MiniAppGIBA.Controller.CMS
{
    [Route("EventGuests")]
    public class EventGuestsController : BaseCMSController
    {
        private readonly IEventGuestService _eventGuestService;
        private readonly ILogger<EventGuestsController> _logger;

        public EventGuestsController(
            IEventGuestService eventGuestService,
            ILogger<EventGuestsController> logger)
        {
            _eventGuestService = eventGuestService;
            _logger = logger;
        }

        /// <summary>
        /// Trang chính quản lý khách mời
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách đơn đăng ký khách mời của sự kiện
        /// </summary>
        [HttpGet("GetAll/{eventId?}")]
        public async Task<IActionResult> GetAll(string? eventId, [FromQuery] byte? status, [FromQuery] string? filterEventId, [FromQuery] string? keyword)
        {
            try
            {
                // Check admin permissions - only GIBA has access
                if (!IsAdmin())
                {
                    return Json(new { data = new List<object>() });
                }

                // GIBA has full access - no filtering needed
                List<EventGuestDTO> result;
                var targetEventId = !string.IsNullOrEmpty(filterEventId) ? filterEventId : eventId;

                if (!string.IsNullOrEmpty(targetEventId) && targetEventId != "all")
                {
                    result = await _eventGuestService.GetEventGuestListsWithRegistrationsAsync(targetEventId, null, null);
                }
                else
                {
                    // Get all event guests across all events
                    result = await _eventGuestService.GetAllEventGuestsWithRegistrationsAsync(null, null);
                }

                // Filter by status if provided
                if (status.HasValue)
                {
                    result = result.Where(eg => eg.Status == status.Value).ToList();
                }

                // Filter by keyword (họ tên, số điện thoại)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var searchKeyword = keyword.Trim().ToLower();
                    result = result.Where(eg => 
                        (!string.IsNullOrEmpty(eg.MemberName) && eg.MemberName.ToLower().Contains(searchKeyword)) ||
                        (!string.IsNullOrEmpty(eg.MemberPhone) && eg.MemberPhone.Contains(searchKeyword)) ||
                        (!string.IsNullOrEmpty(eg.UserZaloId) && eg.UserZaloId.ToLower().Contains(searchKeyword))
                    ).ToList();
                }

                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for event {EventId}", eventId);
                return Json(new { data = new List<object>() });
            }
        }

        /// <summary>
        /// Admin duyệt/từ chối đơn đăng ký khách mời
        /// </summary>
        [HttpPost("Approve/{eventGuestId}")]
        public async Task<IActionResult> Approve(string eventGuestId, [FromBody] ApproveGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                var result = await _eventGuestService.ApproveGuestListAsync(eventGuestId, request);

                var message = request.IsApproved
                    ? "Đã duyệt đơn đăng ký khách mời"
                    : "Đã từ chối đơn đăng ký khách mời";

                return Ok(new { message = message, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving guest list {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn đăng ký khách mời
        /// </summary>
        [HttpGet("Detail/{eventGuestId}")]
        public async Task<IActionResult> GetDetail(string eventGuestId)
        {
            try
            {
                var result = await _eventGuestService.GetEventGuestByIdAsync(eventGuestId);

                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đăng ký" });
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event guest {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin duyệt/từ chối từng thành viên trong đơn đăng ký khách mời
        /// </summary>
        [HttpPost("ApproveGuestItem/{guestListId}")]
        public async Task<IActionResult> ApproveGuestItem(string guestListId, [FromBody] ApproveGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                var result = await _eventGuestService.ApproveGuestListItemAsync(guestListId, request);

                var message = request.IsApproved
                    ? "Đã duyệt thành viên"
                    : "Đã từ chối thành viên";

                return Ok(new { message = message, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving guest list item {GuestListId}", guestListId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

