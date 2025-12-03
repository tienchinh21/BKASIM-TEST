using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Entities.Groups;

namespace MiniAppGIBA.Services.Commons
{
    public class SystemConfigService : Service<SystemConfig>, ISystemConfigService
    {

        public SystemConfigService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
  
        }

        /// <summary>
        /// Lấy nội dung theo Type
        /// </summary>
        public async Task<SystemConfig?> GetByTypeAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;

            return await _repository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Type == type);
        }

        /// <summary>
        /// Cập nhật hoặc tạo mới nội dung theo Type
        /// </summary>
        public async Task<int> UpsertByTypeAsync(string type, string content)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type không được để trống", nameof(type));

            var existing = await GetByTypeAsync(type);

            if (existing != null)
            {
                existing.Content = content;
                existing.UpdatedDate = DateTime.Now;
                return await UpdateAsync(existing);
            }
            else
            {
                var newConfig = new SystemConfig
                {
                    Type = type,
                    Content = content,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                return await CreateAsync(newConfig);
            }
        }

        /// <summary>
        /// Lấy tất cả nội dung theo Type (phân trang)
        /// </summary>
        public async Task<PagedResult<SystemConfig>> GetByTypePagedAsync(string type, IRequestQuery query)
        {
            var records = _repository.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                records = records.Where(c => c.Type == type);
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                records = query.OrderType?.ToLower() == "desc"
                    ? records.OrderByDescending(c => EF.Property<object>(c, query.OrderBy))
                    : records.OrderBy(c => EF.Property<object>(c, query.OrderBy));
            }
            else
            {
                records = records.OrderByDescending(c => c.CreatedDate);
            }

            var totalRecords = records.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            var items = await records
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<SystemConfig>
            {
                Items = items,
                TotalPages = totalPages
            };
        }
        public async Task<string> GetBehaviorRulesFileAsync(EBehaviorRuleType type, string? groupId)
        {
            var result = "";
            if (type == EBehaviorRuleType.BehaviorRulesSupperAdmin)
            {
                var rule = await GetByTypeAsync("BehaviorRules");
                result = rule?.Content ?? "";
            }
            else
            {
                var group = await unitOfWork.GetRepository<Group>().AsQueryable()
                    .FirstOrDefaultAsync(g => g.Id == groupId);
                if (group != null && !string.IsNullOrEmpty(group.BehaviorRulesUrl))
                {
                   
                    result = group?.BehaviorRulesUrl ?? "";
                }
            }

            return result;
        }
    }
}

