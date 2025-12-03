using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.HomePins;
using MiniAppGIBA.Services.HomePins;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    public class HomePinsController : BaseCMSController
    {
        private readonly IHomePinService _homePinService;

        public HomePinsController(IHomePinService homePinService)
        {
            _homePinService = homePinService;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(PinEntityType? filterType = null)
        {
            var result = await _homePinService.GetHomePinsAsync(filterType);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateHomePinRequest request)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var adminId = GetCurrentUserId();
            var result = await _homePinService.PinEntityAsync(request, adminId);

            if (result.IsSuccess)
                return Json(new { success = true, data = result.Data, message = result.Message });

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var adminId = GetCurrentUserId();
            var result = await _homePinService.UnpinEntityAsync(id, adminId);

            if (result.IsSuccess)
                return Json(new { success = true, message = result.Message });

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] ReorderPinsRequest request)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var adminId = GetCurrentUserId();
            var result = await _homePinService.ReorderPinsAsync(request, adminId);

            if (result.IsSuccess)
                return Json(new { success = true, message = result.Message });

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(string id, [FromBody] string notes)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var adminId = GetCurrentUserId();
            var result = await _homePinService.UpdatePinNotesAsync(id, notes, adminId);

            if (result.IsSuccess)
                return Json(new { success = true, data = result.Data, message = result.Message });

            return Json(new { success = false, message = result.Message });
        }

        // GET: /HomePins/GetAvailableEntities?entityType=1
        [HttpGet]
        public async Task<IActionResult> GetAvailableEntities(int entityType)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var result = await _homePinService.GetAvailableEntitiesForAdminAsync((PinEntityType)entityType);

            if (result.IsSuccess)
                return Json(new { success = true, data = result.Data });

            return Json(new { success = false, message = result.Message });
        }
    }
}
