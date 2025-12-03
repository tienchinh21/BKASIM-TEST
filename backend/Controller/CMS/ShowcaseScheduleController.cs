using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Services.ShowCase;
using MiniAppGIBA.Entities.Showcase;
namespace MiniAppGIBA.Controller.CMS
{
    [Route("Showcase")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ShowcaseScheduleController : BaseCMSController
    {
        private readonly IShowcaseService _showcaseService;
        public ShowcaseScheduleController(IShowcaseService showcaseService)
        {
            _showcaseService = showcaseService;
        }

        /// <summary>
        /// Main index page for Showcase Schedule management
        /// </summary>
        /// <returns>Index view</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // TODO: Load any initial data needed for the page
            // ViewBag.Showcases = await _showcaseService.GetAllAsync();
            // For now, just return the view
            return View();
        }

        // [HttpGet("GetPage")]
        // public async Task<IActionResult> GetPage()
        // {
        //     return Json(new { success = true, data = new List<object>() });
        // }
        /// <summary>
        /// Get create form (partial view)
        /// </summary>
        /// <returns>Partial view for creating new showcase schedule</returns>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Button = "Lưu";
            ViewBag.Title = "Thêm mới lịch showcase";
            ViewBag.ScheduleId = null;

            // Return null model for create mode
            return PartialView("_ShowcaseSchedule", (Showcase?)null);
        }

        /// <summary>
        /// Get edit form (partial view) with existing data
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <returns>Partial view for editing showcase schedule</returns>
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            // TODO: Uncomment when service is ready
            
            var schedule = await _showcaseService.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch showcase" });
            }

            ViewBag.Button = "Cập nhật";
            ViewBag.Title = "Cập nhật lịch showcase";
            ViewBag.ScheduleId = id;

            return PartialView("_ShowcaseSchedule", schedule);
            

                
        }
    }
}

