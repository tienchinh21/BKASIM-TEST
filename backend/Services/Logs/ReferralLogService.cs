using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;

namespace MiniAppGIBA.Services.Logs
{
    public class ReferralLogService : Service<ReferralLog>, IReferralLogService
    {
        public ReferralLogService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Tạo referral log (automatic logging khi có giới thiệu)
        /// </summary>
        public async Task<ReferralLog> LogReferralAsync(CreateReferralLogDto dto)
        {
            var log = new ReferralLog
            {
                ReferrerId = dto.ReferrerId,
                RefereeId = dto.RefereeId,
                GroupId = dto.GroupId,
                ReferralCode = dto.ReferralCode,
                Source = dto.Source,
                Metadata = dto.Metadata
            };

            await _repository.AddAsync(log);
            await unitOfWork.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        public async Task<PagedResult<ReferralLogDto>> GetReferralLogsAsync(ReferralLogQueryParameters queryParameters)
        {
            var query = _repository.AsQueryable();

            // Filter
            if (queryParameters.GroupIds != null && queryParameters.GroupIds.Any())
                query = query.Where(l => queryParameters.GroupIds.Contains(l.GroupId));

            if (!string.IsNullOrEmpty(queryParameters.ReferrerId))
                query = query.Where(l => l.ReferrerId == queryParameters.ReferrerId);

            if (!string.IsNullOrEmpty(queryParameters.RefereeId))
                query = query.Where(l => l.RefereeId == queryParameters.RefereeId);

            if (queryParameters.FromDate.HasValue)
                query = query.Where(l => l.CreatedDate >= queryParameters.FromDate.Value);

            if (queryParameters.ToDate.HasValue)
                query = query.Where(l => l.CreatedDate <= queryParameters.ToDate.Value);

            // Keyword search
            if (!string.IsNullOrEmpty(queryParameters.Keyword))
            {
                query = query.Where(l =>
                    (l.ReferralCode != null && l.ReferralCode.Contains(queryParameters.Keyword)) ||
                    (l.Source != null && l.Source.Contains(queryParameters.Keyword)));
            }

            // Total count
            var totalItems = await query.CountAsync();

            // Sorting
            query = !string.IsNullOrEmpty(queryParameters.Sort) && queryParameters.Sort.ToLower() == "asc"
                ? query.OrderBy(l => l.CreatedDate)
                : query.OrderByDescending(l => l.CreatedDate);

            // Pagination
            var logs = await query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            // Join thủ công bằng FK
            var result = new List<ReferralLogDto>();
            foreach (var log in logs)
            {
                var referrer = await unitOfWork.Context.Memberships
                    .FirstOrDefaultAsync(m => m.UserZaloId == log.ReferrerId);
                var referee = await unitOfWork.Context.Memberships
                    .FirstOrDefaultAsync(m => m.UserZaloId == log.RefereeId);
                var group = await unitOfWork.Context.Groups.FindAsync(log.GroupId);

                result.Add(new ReferralLogDto
                {
                    Id = log.Id,
                    ReferrerId = log.ReferrerId,
                    RefereeId = log.RefereeId,
                    GroupId = log.GroupId,
                    ReferralCode = log.ReferralCode,
                    Source = log.Source,
                    Metadata = log.Metadata,
                    CreatedDate = log.CreatedDate,
                    ReferrerName = referrer?.Fullname,
                    RefereeName = referee?.Fullname,
                    GroupName = group?.GroupName
                });
            }

            return new PagedResult<ReferralLogDto>
            {
                Items = result,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParameters.PageSize),
                Page = queryParameters.Page,
                PageSize = queryParameters.PageSize
            };
        }

        /// <summary>
        /// Lấy logs theo nhóm (cho ADMIN chỉ xem nhóm mình quản lý)
        /// </summary>
        public async Task<PagedResult<ReferralLogDto>> GetLogsByGroupsAsync(List<string> groupIds, ReferralLogQueryParameters queryParameters)
        {
            queryParameters.GroupIds = groupIds;
            return await GetReferralLogsAsync(queryParameters);
        }

        /// <summary>
        /// Thống kê top referrers
        /// </summary>
        public async Task<Dictionary<string, int>> GetTopReferrersAsync(List<string>? groupIds = null, int top = 10)
        {
            var logs = await _repository.AsQueryable().ToListAsync();

            if (groupIds != null && groupIds.Any())
                logs = logs.Where(l => groupIds.Contains(l.GroupId)).ToList();

            return logs
                .GroupBy(l => l.ReferrerId)
                .OrderByDescending(g => g.Count())
                .Take(top)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}

