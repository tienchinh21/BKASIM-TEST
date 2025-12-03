using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.CMS;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.Events;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("EventGift")]
    public class EventGiftController : BaseCMSController
    {
        private readonly IEventGiftService _eventGiftService;
        private readonly IEventService _eventService;
        private readonly ILogger<EventGiftController> _logger;

        public EventGiftController(
            IEventGiftService eventGiftService,
            IEventService eventService,
            ILogger<EventGiftController> logger)
        {
            _eventGiftService = eventGiftService;
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách phần quà
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Load active events for filter dropdown
                var events = await _eventService.GetActiveEventsAsync();
                ViewBag.Events = events;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading event gifts index page");
                return View("Error");
            }
        }

        /// <summary>
        /// Lấy dữ liệu phân trang cho DataTable
        /// </summary>
        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] EventGiftQueryParameters query)
        {
            try
            {
                var result = await _eventGiftService.GetEventGiftsAsync(query);

                var response = new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = result.TotalItems,
                    recordsFiltered = result.TotalItems,
                    data = result.Items.Select(item => new
                    {
                        id = item.Id,
                        giftName = item.GiftName,
                        eventTitle = item.EventTitle,
                        quantity = item.Quantity,
                        images = item.Images,
                        isActive = item.IsActive,
                        createdDate = item.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                    })
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event gifts page data");
                return Json(new { error = "Có lỗi xảy ra khi tải dữ liệu" });
            }
        }

        /// <summary>
        /// Hiển thị form tạo phần quà
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> GetCreateForm()
        {
            try
            {
                var events = await _eventService.GetActiveEventsAsync();
                ViewBag.Events = events;
                ViewBag.IsEdit = false;
                ViewBag.Title = "Tạo Phần Quà Mới";
                ViewBag.Button = "Tạo Phần Quà";

                var model = new CreateEventGiftRequest();
                return PartialView("Partials/_EventGiftForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create event gift form");
                return PartialView("Error");
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa phần quà
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> GetEditForm(string id)
        {
            try
            {
                var gift = await _eventGiftService.GetEventGiftByIdAsync(id);
                if (gift == null)
                {
                    return NotFound();
                }

                var events = await _eventService.GetActiveEventsAsync();
                ViewBag.Events = events;
                ViewBag.IsEdit = true;
                ViewBag.GiftId = id;
                ViewBag.Title = "Chỉnh Sửa Phần Quà";
                ViewBag.Button = "Cập Nhật";

                // Convert to UpdateEventGiftRequest for form binding
                var model = new UpdateEventGiftRequest
                {
                    Id = gift.Id,
                    EventId = gift.EventId,
                    GiftName = gift.GiftName,
                    Quantity = gift.Quantity,
                    IsActive = gift.IsActive
                };

                // Pass images for display
                ViewBag.Images = gift.Images;

                return PartialView("Partials/_EventGiftForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit event gift form for {GiftId}", id);
                return PartialView("Error");
            }
        }

        /// <summary>
        /// Tạo phần quà mới
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateEventGiftRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var result = await _eventGiftService.CreateEventGiftAsync(request);
                return Json(new { success = true, message = "Tạo phần quà thành công", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event gift");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo phần quà" });
            }
        }

        /// <summary>
        /// Cập nhật phần quà
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UpdateEventGiftRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var result = await _eventGiftService.UpdateEventGiftAsync(request.Id, request);
                return Json(new { success = true, message = "Cập nhật phần quà thành công", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event gift {GiftId}", request.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật phần quà" });
            }
        }

        /// <summary>
        /// Xóa phần quà
        /// </summary>
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _eventGiftService.DeleteEventGiftAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy phần quà" });
                }

                return Json(new { success = true, message = "Xóa phần quà thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event gift {GiftId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa phần quà" });
            }
        }

        /// <summary>
        /// Thay đổi trạng thái phần quà
        /// </summary>
        [HttpPost("ToggleStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var result = await _eventGiftService.ToggleEventGiftStatusAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy phần quà" });
                }

                return Json(new { success = true, message = "Thay đổi trạng thái phần quà thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling event gift status {GiftId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái phần quà" });
            }
        }
    }
}
