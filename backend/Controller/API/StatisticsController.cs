using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Appointment;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Controller.API
{
    /// <summary>
    /// API thống kê số liệu cá nhân
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : BaseAPIController
    {
        private readonly ILogger<StatisticsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public StatisticsController(
            ILogger<StatisticsController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// GET /api/statistics/user-stats - Lấy thống kê số liệu cá nhân
        /// </summary>
        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                // Kiểm tra authentication
                if (!IsAuthenticated())
                {
                    return Error("Unauthorized", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin user", 401);
                }

                // 1. Sự kiện: Số lần check-in sự kiện
                // Status == ERegistrationStatus.CheckedIn (2) và CheckInStatus == ECheckInStatus.CheckedIn (2)
                var eventCheckInCount = await _unitOfWork.Context.EventRegistrations
                    .CountAsync(er => er.UserZaloId == userZaloId 
                        && er.Status == (int)ERegistrationStatus.CheckedIn 
                        && er.CheckInStatus == ECheckInStatus.CheckedIn);

                // 2. Mời khách: Số lần và tổng số lượng
                var eventGuests = await _unitOfWork.Context.EventGuests
                    .Where(eg => eg.UserZaloId == userZaloId)
                    .ToListAsync();
                
                var guestInviteCount = eventGuests.Count;
                var totalGuestNumber = eventGuests.Sum(eg => eg.GuestNumber);

                // 3. Ref: Số lần và giá trị (gửi/nhận)
                var refsSent = await _unitOfWork.Context.Refs
                    .Where(r => r.RefFrom == userZaloId)
                    .ToListAsync();
                
                var refsReceived = await _unitOfWork.Context.Refs
                    .Where(r => r.RefTo == userZaloId)
                    .ToListAsync();

                var refSentCount = refsSent.Count;
                var refSentValue = refsSent.Sum(r => r.Value);
                var refReceivedCount = refsReceived.Count;
                var refReceivedValue = refsReceived.Sum(r => r.Value);

                // 4. Đặt hẹn: Số lần xác nhận
                // Status == EAppointmentStatus.Confirmed (2) và (AppointmentFrom == userZaloId hoặc AppointmentTo == userZaloId)
                var appointmentCount = await _unitOfWork.Context.Appointments
                    .CountAsync(a => a.Status == (byte)EAppointmentStatus.Confirmed 
                        && (a.AppointmentFrom == userZaloId || a.AppointmentTo == userZaloId));

                return Success(new
                {
                    events = new
                    {
                        checkInCount = eventCheckInCount
                    },
                    guestInvites = new
                    {
                        count = guestInviteCount,
                        totalGuestNumber = totalGuestNumber
                    },
                    refs = new
                    {
                        sent = new
                        {
                            count = refSentCount,
                            value = refSentValue
                        },
                        received = new
                        {
                            count = refReceivedCount,
                            value = refReceivedValue
                        }
                    },
                    appointments = new
                    {
                        confirmedCount = appointmentCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return Error("Có lỗi xảy ra khi lấy thống kê", 500);
            }
        }
    }
}

