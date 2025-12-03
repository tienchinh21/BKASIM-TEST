using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.Appointment;
using MiniAppGIBA.Models.Queries.Appointment;
using MiniAppGIBA.Services.Appointment;
namespace MiniAppGIBA.Controller.CMS

{
    [Route("Appointment")]
    public class AppointmentController : BaseCMSController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View("~/Views/Appointment/Index.cshtml");
        }
        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage([FromQuery] AppointmentQueryParams query)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var appointments = await _appointmentService.GetPage(query);
            return Json(new { success = true, data = appointments.Items, totalCount = appointments.TotalItems });
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var appointment = await _appointmentService.GetAppointmentDetailById(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
            }

            return Json(new { success = true, data = appointment });
        }
    }
}