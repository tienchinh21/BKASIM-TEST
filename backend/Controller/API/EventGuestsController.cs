using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.EventGuests;

namespace MiniAppGIBA.Controller.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventGuestsController : ControllerBase
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
        [HttpGet("GetEventByPhone/{phone}")]
        public async Task<IActionResult> GetEventByPhone(string phone)
        {
            try
            {
                var result = await _eventGuestService.GetEventByPhoneAsync(phone);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by phone {Phone}", phone);
                return BadRequest(new { message = ex.Message });
            }
        }

	        [HttpGet("GetConfirmedEventsByPhone/{phone}")]
	        public async Task<IActionResult> GetConfirmedEventsByPhone(string phone)
	        {
	            try
	            {
	                var result = await _eventGuestService.GetConfirmedEventsByPhoneAsync(phone);

	                return Ok(result);
	            }
	            catch (Exception ex)
	            {
	                _logger.LogError(ex, "Error getting confirmed events by phone {Phone}", phone);
	                return BadRequest(new { message = ex.Message });
	            }
	        }
        /// <summary>
        /// User đăng ký danh sách khách mời cho sự kiện
        /// </summary>
        [HttpPost("Register/{eventId}")]
        [Authorize]
        public async Task<IActionResult> RegisterGuestList(string eventId, [FromBody] RegisterGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                await _eventGuestService.RegisterGuestListAsync(eventId, userZaloId, request);

                return Ok(new
                {
                    code = 0,
                    message = "Đăng ký danh sách khách mời thành công! Vui lòng chờ admin duyệt."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering guest list for event {EventId}", eventId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// User hủy đơn đăng ký danh sách khách mời của mình
        /// </summary>
        [HttpPost("Cancel/{eventGuestId}")]
        [Authorize]
        public async Task<IActionResult> CancelGuestList(string eventGuestId, [FromBody] CancelGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra quyền sở hữu
                var eventGuest = await _eventGuestService.GetEventGuestByIdAsync(eventGuestId);
                if (eventGuest == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đăng ký" });
                }

                if (eventGuest.UserZaloId != userZaloId)
                {
                    return Forbid();
                }

                var result = await _eventGuestService.CancelGuestListAsync(eventGuestId, request.CancelReason ?? "Người dùng hủy");

                return Ok(new
                {
                    message = "Đã hủy đơn đăng ký khách mời",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling guest list {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Khách mời trong danh sách tự hủy đăng ký của mình
        /// </summary>
        [HttpPost("CancelItem/{guestListId}")]
        public async Task<IActionResult> CancelGuestListItem(string guestListId, [FromBody] CancelGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _eventGuestService.CancelGuestListItemAsync(guestListId, request.CancelReason ?? "Khách mời tự hủy");

                return Ok(new
                {
                    message = "Đã hủy đăng ký khách mời",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling guest list item {GuestListId}", guestListId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách đơn đăng ký khách mời của user
        /// </summary>
        [HttpGet("MyGuestLists")]
        [Authorize]
        public async Task<IActionResult> GetMyGuestLists([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] byte? status = null)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventGuestService.GetMyGuestListsAsync(userZaloId, page, pageSize, keyword, status);

                return Ok(new
                {
                    code = 0,
                    message = "Lấy danh sách đơn đăng ký khách mời thành công",
                    data = result.Items,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for user");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn đăng ký khách mời
        /// </summary>
        [HttpGet("{eventGuestId}")]
        [Authorize]
        public async Task<IActionResult> GetEventGuest(string eventGuestId)
        {
            try
            {
                var result = await _eventGuestService.GetEventGuestByIdAsync(eventGuestId);

                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đăng ký" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event guest {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Lấy tất cả đơn đăng ký khách mời theo eventId
        /// </summary>
        [HttpGet("Event/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetGuestListsByEvent(string eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] byte? status = null)
        {
            try
            {
                var result = await _eventGuestService.GetEventGuestListsAsync(eventId, page, pageSize, keyword, status);

                return Ok(new
                {
                    code = 0,
                    message = "Lấy danh sách đơn đăng ký khách mời theo sự kiện thành công",
                    data = result.Items,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for event {EventId}", eventId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn đăng ký khách mời
        /// </summary>
        [HttpGet("Detail/{eventGuestId}")]
        [Authorize]
        public async Task<IActionResult> GetGuestListDetail(string eventGuestId)
        {
            try
            {
                var result = await _eventGuestService.GetEventGuestByIdAsync(eventGuestId);
                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đăng ký khách mời" });
                }

                return Ok(new
                {
                    code = 0,
                    message = "Lấy chi tiết đơn đăng ký khách mời thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest list detail {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật danh sách khách mời (chỉ cho phép khi chưa được duyệt)
        /// </summary>
        [HttpPut("Update/{eventGuestId}")]
        [Authorize]
        public async Task<IActionResult> UpdateGuestList(string eventGuestId, [FromBody] RegisterGuestListRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventGuestService.UpdateGuestListAsync(eventGuestId, userZaloId, request);

                return Ok(new
                {
                    code = 0,
                    message = "Cập nhật danh sách khách mời thành công!",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guest list {EventGuestId}", eventGuestId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    
}

