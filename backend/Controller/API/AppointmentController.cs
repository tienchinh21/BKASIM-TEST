using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.Appointment;
using MiniAppGIBA.Models.Queries.Appointment;
using MiniAppGIBA.Models.Request.Appointments;
using MiniAppGIBA.Services.Appointment;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Constants;
namespace MiniAppGIBA.Controller.API

{
    [Route("api/[controller]")]
    public class AppointmentController : BaseAPIController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] string? type = null, [FromQuery] int? status = null)

    {
        var userId = GetCurrentUserZaloId();
        Console.WriteLine(userId);
        if (userId == null)
        {
            return BadRequest(new { message = "Người dùng không tồn tại" });
        }
        
        var result = await _appointmentService.GetAppointmentFilter(userId,type,status);
        return Success(result);
    }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentRequest model)
        {
            var result = await _appointmentService.Create(model);
            if (result == 0)
            {
                return BadRequest(new { message = "Lỗi khi tạo lịch hẹn" });
            }
            return Success(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateAppointmentRequest request)
        {
            var userId = GetCurrentUserZaloId();
            if (userId == null)
            {
                return BadRequest(new { message = "Người dùng không tồn tại" });
            }

            if (request.Id != id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            var result = await _appointmentService.Update(id, userId, request);
            if (!result)
            {
                return BadRequest(new { message = "Không thể cập nhật lịch hẹn. Chỉ người tạo lịch hẹn mới được phép cập nhật." });
            }

            return Success(new { message = "Cập nhật lịch hẹn thành công" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAppointmentStatusRequest request)
        {
            if (request.Id != id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            // Kiểm tra nếu status == 3 (Cancelled) thì phải có CancelReason
            if (request.Status == (byte)EAppointmentStatus.Cancelled && string.IsNullOrEmpty(request.CancelReason))
            {
                return BadRequest(new { message = "Khi hủy lịch hẹn phải có lý do hủy" });
            }

            var result = await _appointmentService.UpdateStatus(id, request);
            if (!result)
            {
                return BadRequest(new { message = "Không thể cập nhật trạng thái lịch hẹn" });
            }

            return Success(new { message = "Cập nhật trạng thái lịch hẹn thành công" });
        }
    }
}