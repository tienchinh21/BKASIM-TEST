using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Services.Meetings;
using MiniAppGIBA.Entities.Meetings;

namespace MiniAppGIBA.Controller.CMS
{
    [Route("Meeting")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class MeetingController : BaseCMSController
    {
        private readonly IMeetingService _meetingService;

        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        /// <summary>
        /// Main index page for Meeting management
        /// </summary>
        /// <returns>Index view</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get create form (partial view)
        /// </summary>
        /// <returns>Partial view for creating new meeting</returns>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Button = "Lưu";
            ViewBag.Title = "Thêm mới lịch họp";
            ViewBag.MeetingId = null;

            return PartialView("_Meeting", (Meeting?)null);
        }

        /// <summary>
        /// Get edit form (partial view) with existing data
        /// </summary>
        /// <param name="id">Meeting ID</param>
        /// <returns>Partial view for editing meeting</returns>
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var meeting = await _meetingService.GetByIdAsync(id);
            if (meeting == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch họp" });
            }

            ViewBag.Button = "Cập nhật";
            ViewBag.Title = "Cập nhật lịch họp";
            ViewBag.MeetingId = id;

            return PartialView("_Meeting", meeting);
        }
    }
}

