using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Request.EventRegistrations;
using MiniAppGIBA.Services.EventRegistrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventRegistrationsController : ControllerBase
    {
        private readonly IEventRegistrationService _eventRegistrationService;
        private readonly ILogger<EventRegistrationsController> _logger;

        public EventRegistrationsController(
            IEventRegistrationService eventRegistrationService,
            ILogger<EventRegistrationsController> logger)
        {
            _eventRegistrationService = eventRegistrationService;
            _logger = logger;
        }

        [HttpPost("CheckIn/{eventId}")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInData data, string eventId)
        {
            try
            {
                var result = await _eventRegistrationService.CheckInAsync(data.Code, eventId);
                return Ok(new { code = 0, message = "CheckIn thành công!", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for event {EventId} with code {Code}", eventId, data.Code);
                return Ok(new { code = 1, message = ex.Message });
            }
        }

        [HttpPost("CheckInRange/{eventId}")]
        public async Task<IActionResult> CheckInRange([FromBody] CheckInRangeData data, string eventId)
        {
            try
            {
                var result = await _eventRegistrationService.CheckInMultipleAsync(data.Code, eventId);
                return Ok(new { code = 0, message = "CheckIn thành công!", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk check-in for event {EventId}", eventId);
                return Ok(new { code = 1, message = ex.Message });
            }
        }

        [HttpPost("CancelByCode/{eventId}")]
        public async Task<IActionResult> CancelByCode([FromBody] CheckInData data, string eventId)
        {
            try
            {
                var result = await _eventRegistrationService.CancelByCodeAsync(data.Code, eventId);
                if (result)
                {
                    return Ok(new { code = 0, message = "Đã hủy tham dự thành công!" });
                }
                return Ok(new { code = 1, message = "Không tìm thấy đăng ký để hủy" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling registration for event {EventId} with code {Code}", eventId, data.Code);
                return Ok(new { code = 1, message = ex.Message });
            }
        }

        [HttpPost("Participants/Export")]
        public async Task<IActionResult> ExportParticipants([FromBody] string eventId)
        {
            try
            {
                var excelData = await _eventRegistrationService.ExportParticipantsAsync(eventId);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"event_participants_{eventId}.xlsx");
            }
            catch (NotImplementedException)
            {
                return BadRequest("Export functionality not implemented yet");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting participants for event {EventId}", eventId);
                return BadRequest("Có lỗi xảy ra khi xuất Excel");
            }
        }

        // Mini app APIs
        [HttpPost("Register/{eventId}")]
        [Authorize]
        public async Task<IActionResult> RegisterEvent(string eventId, [FromBody] RegisterEventRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { message = string.Join("; ", errors) });
                }

                // Get UserZaloId from JWT token
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var registration = await _eventRegistrationService.RegisterEventAsync(eventId, userZaloId, request);
                return Ok(new
                {
                    message = "Đăng ký sự kiện thành công!",
                    data = new
                    {
                        registrationId = registration.Id,
                        eventId = registration.EventId,
                        checkInCode = registration.CheckInCode,
                        status = registration.Status,
                        statusText = registration.StatusText,
                        createdDate = registration.CreatedDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering event {EventId} for user {UserZaloId}", eventId, User.FindFirst("UserZaloId")?.Value);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("MyRegistrations")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistrations()
        {
            try
            {
                // Get UserZaloId from JWT token
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventRegistrationService.GetUserEventRegistrationsAsync(userZaloId);
                return Ok(new
                {
                    message = "Lấy danh sách đăng ký thành công!",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations for user {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy danh sách đăng ký" });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy tất cả đơn đăng ký của user (cả đơn lẻ và khách mời nhóm)
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số items mỗi trang</param>
        /// <param name="type">1=Đăng ký đơn lẻ, 2=Đăng ký khách mời nhóm, null=Tất cả</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="status">Trạng thái (0=Pending, 1=Approved, 2=CheckedIn/Rejected, 3=Cancelled)</param>
        [HttpGet("MyAllRegistrations")]
        [Authorize]
        public async Task<IActionResult> GetMyAllRegistrations(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] int? type = null,
            [FromQuery] string? keyword = null,
            [FromQuery] byte? status = null)
        {
            try
            {
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventRegistrationService.GetAllUserRegistrationsAsync(
                    userZaloId, page, pageSize, type, keyword, status);

                return Ok(new
                {
                    code = 0,
                    message = "Lấy danh sách đăng ký thành công",
                    data = result.Items,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all registrations for user {UserZaloId}", User.FindFirst("UserZaloId")?.Value);
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy danh sách đăng ký" });
            }
        }

        [HttpDelete("Cancel/{eventId}")]
        [Authorize]
        public async Task<IActionResult> CancelRegistration(string eventId)
        {
            try
            {
                // Get UserZaloId from JWT token
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventRegistrationService.CancelEventRegistrationAsync(eventId, userZaloId);
                if (result)
                {
                    return Ok(new { message = "Hủy đăng ký thành công!" });
                }
                return BadRequest(new { message = "Không tìm thấy đăng ký để hủy" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling registration for event {EventId} and user {UserZaloId}", eventId, User.FindFirst("UserZaloId")?.Value);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [MINI APP] Lấy chi tiết sự kiện với logic hiển thị theo quyền
        /// </summary>
        [HttpGet("EventDetail/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetEventDetail(string eventId)
        {
            try
            {
                // Get UserZaloId from JWT token
                var userZaloId = User.FindFirst("UserZaloId")?.Value;
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var result = await _eventRegistrationService.GetEventDetailForUserAsync(eventId, userZaloId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event detail {EventId} for user {UserZaloId}", eventId, User.FindFirst("UserZaloId")?.Value);
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CheckInData
    {
        public string Code { get; set; } = string.Empty;
    }

    public class CheckInRangeData
    {
        public List<string> Code { get; set; } = new List<string>();
    }
}
