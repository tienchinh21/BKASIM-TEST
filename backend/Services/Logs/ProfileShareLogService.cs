using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;

namespace MiniAppGIBA.Services.Logs
{
    public class ProfileShareLogService : Service<ProfileShareLog>, IProfileShareLogService
    {
        public ProfileShareLogService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Tạo profile share log (automatic logging khi chia sẻ profile)
        /// </summary>
        public async Task<ProfileShareLog> LogProfileShareAsync(CreateProfileShareLogDto dto)
        {
            var log = new ProfileShareLog
            {
                SharerId = dto.SharerId,
                ReceiverId = dto.ReceiverId,
                GroupId = dto.GroupId,
                SharedData = dto.SharedData,
                ShareMethod = dto.ShareMethod,
                Metadata = dto.Metadata
            };

            await _repository.AddAsync(log);
            await unitOfWork.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        public async Task<PagedResult<ProfileShareLogDto>> GetProfileShareLogsAsync(ProfileShareLogQueryParameters queryParameters)
        {
            var query = _repository.AsQueryable();

            // Filter
            if (queryParameters.GroupIds != null && queryParameters.GroupIds.Any())
                query = query.Where(l => queryParameters.GroupIds.Contains(l.GroupId));

            if (!string.IsNullOrEmpty(queryParameters.SharerId))
                query = query.Where(l => l.SharerId == queryParameters.SharerId);

            if (!string.IsNullOrEmpty(queryParameters.ReceiverId))
                query = query.Where(l => l.ReceiverId == queryParameters.ReceiverId);

            if (queryParameters.FromDate.HasValue)
                query = query.Where(l => l.CreatedDate >= queryParameters.FromDate.Value);

            if (queryParameters.ToDate.HasValue)
                query = query.Where(l => l.CreatedDate <= queryParameters.ToDate.Value);

            // Keyword search
            if (!string.IsNullOrEmpty(queryParameters.Keyword))
            {
                query = query.Where(l =>
                    (l.ShareMethod != null && l.ShareMethod.Contains(queryParameters.Keyword)) ||
                    (l.SharedData != null && l.SharedData.Contains(queryParameters.Keyword)));
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
            var result = new List<ProfileShareLogDto>();
            foreach (var log in logs)
            {
                var sharer = await unitOfWork.Context.Memberships
                    .FirstOrDefaultAsync(m => m.UserZaloId == log.SharerId);
                var receiver = await unitOfWork.Context.Memberships
                    .FirstOrDefaultAsync(m => m.UserZaloId == log.ReceiverId);
                var group = await unitOfWork.Context.Groups.FindAsync(log.GroupId);

                result.Add(new ProfileShareLogDto
                {
                    Id = log.Id,
                    SharerId = log.SharerId,
                    ReceiverId = log.ReceiverId,
                    GroupId = log.GroupId,
                    SharedData = log.SharedData,
                    ShareMethod = log.ShareMethod,
                    Metadata = log.Metadata,
                    CreatedDate = log.CreatedDate,
                    SharerName = sharer?.Fullname,
                    ReceiverName = receiver?.Fullname,
                    GroupName = group?.GroupName
                });
            }

            return new PagedResult<ProfileShareLogDto>
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
        public async Task<PagedResult<ProfileShareLogDto>> GetLogsByGroupsAsync(List<string> groupIds, ProfileShareLogQueryParameters queryParameters)
        {
            queryParameters.GroupIds = groupIds;
            return await GetProfileShareLogsAsync(queryParameters);
        }

        /// <summary>
        /// Thống kê top sharers
        /// </summary>
        public async Task<Dictionary<string, int>> GetTopSharersAsync(List<string>? groupIds = null, int top = 10)
        {
            var logs = await _repository.AsQueryable().ToListAsync();

            if (groupIds != null && groupIds.Any())
                logs = logs.Where(l => groupIds.Contains(l.GroupId)).ToList();

            return logs
                .GroupBy(l => l.SharerId)
                .OrderByDescending(g => g.Count())
                .Take(top)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}

