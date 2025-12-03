using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Groups;
using System.Globalization;

namespace MiniAppGIBA.Controller.API
{
    /// <summary>
    /// API Dashboard cho Zalo Mini App - Thống kê cá nhân của từng thành viên
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/dashboard")]
    [ApiController]
    public class MembershipDashboardController : BaseAPIController
    {
        private readonly ILogger<MembershipDashboardController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public MembershipDashboardController(
            ILogger<MembershipDashboardController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// GET /api/dashboard/stats - Lấy thống kê dashboard cá nhân cho user hiện tại
        /// </summary>
        /// <param name="period">weekly | monthly | today (default: weekly)</param>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] string period = "weekly")
        {
            try
            {
                // Kiểm tra authentication
                if (!IsAuthenticated())
                {
                    return StatusCode(401, new
                    {
                        code = 401,
                        message = "Unauthorized",
                        data = (object?)null
                    });
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return StatusCode(401, new
                    {
                        code = 401,
                        message = "Unauthorized",
                        data = (object?)null
                    });
                }

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfYear = new DateTime(now.Year, 1, 1);

                // === REPOSITORIES ===
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var eventRegistrationRepo = _unitOfWork.GetRepository<EventRegistration>();
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();

                // === SUMMARY CALCULATIONS ===

                // 1. Total Refs Sent (tháng hiện tại)
                var totalRefsSent = await refRepo.AsQueryable()
                    .CountAsync(r => r.RefFrom == userZaloId && r.CreatedDate >= startOfMonth);

                // 2. Total Refs Received (tháng hiện tại)
                var totalRefsReceived = await refRepo.AsQueryable()
                    .CountAsync(r => r.RefTo == userZaloId && r.CreatedDate >= startOfMonth);

                // 3. Total Revenue (tổng giá trị các ref completed - tháng hiện tại)
                var totalRevenue = await refRepo.AsQueryable()
                    .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId)
                                && r.Status == 3 // Completed
                                && r.CreatedDate >= startOfMonth)
                    .SumAsync(r => r.Value);

                // 4. Total Events (năm nay)
                var totalEvents = await eventRegistrationRepo.AsQueryable()
                    .CountAsync(er => er.UserZaloId == userZaloId
                                   && er.Status != 3 // Exclude cancelled
                                   && er.CreatedDate >= startOfYear);

                // 5. Total Ref Value (tháng hiện tại - tất cả refs)
                var totalRefValue = await refRepo.AsQueryable()
                    .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId)
                                && r.CreatedDate >= startOfMonth)
                    .SumAsync(r => r.Value);

                // 6. Total Groups
                var totalGroups = await membershipGroupRepo.AsQueryable()
                    .CountAsync(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true);

                // 7. Average Rating (trung bình rating của các ref gửi đi - tháng hiện tại)
                var refsSentWithRating = await refRepo.AsQueryable()
                    .Where(r => r.RefFrom == userZaloId
                                && r.CreatedDate >= startOfMonth
                                && r.Rating.HasValue)
                    .ToListAsync();

                var averageRating = refsSentWithRating.Count > 0
                    ? Math.Round(refsSentWithRating.Average(r => r.Rating.Value), 2)
                    : 0;

                // 8. Feedback Rate (tỉ lệ phản hồi - số ref gửi đi được phản hồi / tổng ref gửi đi)
                var refsSentTotal = await refRepo.AsQueryable()
                    .Where(r => r.RefFrom == userZaloId && r.CreatedDate >= startOfMonth)
                    .ToListAsync();

                var refsSentWithFeedback = refsSentTotal.Count(r => r.Rating.HasValue);
                var feedbackRate = refsSentTotal.Count > 0
                    ? Math.Round((double)refsSentWithFeedback / refsSentTotal.Count * 100, 2)
                    : 0;

                // === CHART DATA ===
                var chartData = await GenerateChartDataAsync(userZaloId, period, refRepo);

                // === RESPONSE ===
                return Ok(new
                {
                    code = 0,
                    message = "Success",
                    data = new
                    {
                        summary = new
                        {
                            totalRefsSent = totalRefsSent,
                            totalRefsReceived = totalRefsReceived,
                            totalRevenue = (long)totalRevenue, // Convert to VNĐ (long)
                            totalEvents = totalEvents,
                            totalRefValue = (long)totalRefValue,
                            totalGroups = totalGroups,
                            averageRating = averageRating,
                            feedbackRate = feedbackRate
                        },
                        chartData = chartData
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for user");
                return StatusCode(500, new
                {
                    code = 500,
                    message = "Internal Server Error",
                    data = (object?)null
                });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Generate chart data based on period (weekly, monthly, today)
        /// </summary>
        private async Task<List<object>> GenerateChartDataAsync(string userZaloId, string period, IRepository<Ref> refRepo)
        {
            var now = DateTime.Now;
            var chartData = new List<object>();

            switch (period.ToLower())
            {
                case "today":
                    // Trả về data theo giờ trong ngày hôm nay (24 data points)
                    var startOfToday = now.Date;
                    var endOfToday = startOfToday.AddDays(1);

                    var refsToday = await refRepo.AsQueryable()
                        .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId)
                                    && r.CreatedDate >= startOfToday
                                    && r.CreatedDate < endOfToday)
                        .ToListAsync();

                    // Group by hour
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var hourStart = startOfToday.AddHours(hour);
                        var hourEnd = hourStart.AddHours(1);

                        var refsSentInHour = refsToday.Count(r => r.RefFrom == userZaloId
                                                                   && r.CreatedDate >= hourStart
                                                                   && r.CreatedDate < hourEnd);
                        var refsReceivedInHour = refsToday.Count(r => r.RefTo == userZaloId
                                                                       && r.CreatedDate >= hourStart
                                                                       && r.CreatedDate < hourEnd);

                        chartData.Add(new
                        {
                            date = hourStart.ToString("yyyy-MM-dd"),
                            day = $"{hour:D2}h",
                            refSent = refsSentInHour,
                            refReceived = refsReceivedInHour
                        });
                    }
                    break;

                case "monthly":
                    // Trả về data theo tuần trong tháng (4-5 data points)
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);

                    var refsThisMonth = await refRepo.AsQueryable()
                        .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId)
                                    && r.CreatedDate >= startOfMonth
                                    && r.CreatedDate < endOfMonth)
                        .ToListAsync();

                    // Group by week
                    var currentWeekStart = startOfMonth;
                    var weekNumber = 1;

                    while (currentWeekStart < endOfMonth)
                    {
                        var weekEnd = currentWeekStart.AddDays(7);
                        if (weekEnd > endOfMonth) weekEnd = endOfMonth;

                        var refsSentInWeek = refsThisMonth.Count(r => r.RefFrom == userZaloId
                                                                       && r.CreatedDate >= currentWeekStart
                                                                       && r.CreatedDate < weekEnd);
                        var refsReceivedInWeek = refsThisMonth.Count(r => r.RefTo == userZaloId
                                                                           && r.CreatedDate >= currentWeekStart
                                                                           && r.CreatedDate < weekEnd);

                        chartData.Add(new
                        {
                            date = currentWeekStart.ToString("yyyy-MM-dd"),
                            day = $"Tuần {weekNumber}",
                            refSent = refsSentInWeek,
                            refReceived = refsReceivedInWeek
                        });

                        currentWeekStart = weekEnd;
                        weekNumber++;
                    }
                    break;

                case "weekly":
                default:
                    // Trả về 7 ngày gần nhất (từ T2 đến CN)
                    var startOfWeek = now.Date.AddDays(-6); // 7 ngày gần nhất

                    var refsThisWeek = await refRepo.AsQueryable()
                        .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId)
                                    && r.CreatedDate >= startOfWeek)
                        .ToListAsync();

                    // Generate 7 days
                    for (int i = 0; i < 7; i++)
                    {
                        var currentDay = startOfWeek.AddDays(i);
                        var nextDay = currentDay.AddDays(1);

                        var refsSentInDay = refsThisWeek.Count(r => r.RefFrom == userZaloId
                                                                     && r.CreatedDate >= currentDay
                                                                     && r.CreatedDate < nextDay);
                        var refsReceivedInDay = refsThisWeek.Count(r => r.RefTo == userZaloId
                                                                         && r.CreatedDate >= currentDay
                                                                         && r.CreatedDate < nextDay);

                        // Get day name in Vietnamese
                        var dayName = GetVietnameseDayName(currentDay.DayOfWeek);

                        chartData.Add(new
                        {
                            date = currentDay.ToString("yyyy-MM-dd"),
                            day = dayName,
                            refSent = refsSentInDay,
                            refReceived = refsReceivedInDay
                        });
                    }
                    break;
            }

            return chartData;
        }

        /// <summary>
        /// Get Vietnamese day name from DayOfWeek
        /// </summary>
        private string GetVietnameseDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }

        #endregion
    }
}

