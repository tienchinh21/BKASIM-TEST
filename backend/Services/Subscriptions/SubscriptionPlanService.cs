using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Subscriptions;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using MiniAppGIBA.Models.Requests.Common;
using MiniAppGIBA.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Services.Subscriptions
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        protected readonly IUnitOfWork _unitOfWork;

        public SubscriptionPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<SubscriptionPlanDTO>> GetPagedAsync(BaseQueryParameters queryParams)
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            var query = repo.AsQueryable();

            // Filter by keyword
            if (!string.IsNullOrEmpty(queryParams.Keyword))
            {
                query = query.Where(x => x.PlanName.Contains(queryParams.Keyword) ||
                                        (x.Description != null && x.Description.Contains(queryParams.Keyword)));
            }

            // Filter by IsActive
            if (queryParams.Status.HasValue)
            {
                query = query.Where(x => x.IsActive == queryParams.Status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(x => new SubscriptionPlanDTO
                {
                    Id = x.Id,
                    PlanName = x.PlanName,
                    Description = x.Description,
                    DurationDays = x.DurationDays,
                    Price = x.Price,
                    IsActive = x.IsActive,
                    Features = x.Features,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResult<SubscriptionPlanDTO>
            {
                Items = items,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize),
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<List<SubscriptionPlanDTO>> GetActivePlansAsync()
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            return await repo.AsQueryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.PlanName)
                .Select(x => new SubscriptionPlanDTO
                {
                    Id = x.Id,
                    PlanName = x.PlanName,
                    Description = x.Description,
                    DurationDays = x.DurationDays,
                    Price = x.Price,
                    IsActive = x.IsActive,
                    Features = x.Features,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<List<SubscriptionPlanDTO>> GetPlansByGroupIdAsync(string groupId)
        {
            var configRepo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            return await configRepo.AsQueryable()
                .Where(c => c.GroupId == groupId && c.IsActive)
                .Join(planRepo.AsQueryable(),
                    config => config.SubscriptionPlanId,
                    plan => plan.Id,
                    (config, plan) => new SubscriptionPlanDTO
                    {
                        Id = plan.Id,
                        PlanName = plan.PlanName,
                        Description = plan.Description,
                        DurationDays = plan.DurationDays,
                        Price = plan.Price,
                        IsActive = plan.IsActive,
                        Features = plan.Features,
                        CreatedDate = plan.CreatedDate,
                        UpdatedDate = plan.UpdatedDate
                    })
                .OrderBy(x => x.PlanName)
                .ToListAsync();
        }

        public async Task<SubscriptionPlanDTO> CreateAsync(CreateSubscriptionPlanDTO request)
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            var entity = new SubscriptionPlan
            {
                PlanName = request.PlanName,
                Description = request.Description,
                DurationDays = request.DurationDays,
                Price = request.Price,
                IsActive = request.IsActive,
                Features = request.Features
            };

            await repo.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new SubscriptionPlanDTO
            {
                Id = entity.Id,
                PlanName = entity.PlanName,
                Description = entity.Description,
                DurationDays = entity.DurationDays,
                Price = entity.Price,
                IsActive = entity.IsActive,
                Features = entity.Features,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<SubscriptionPlanDTO> UpdateAsync(UpdateSubscriptionPlanDTO request)
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            var entity = await repo.FindByIdAsync(request.Id);

            if (entity == null)
                throw new ArgumentException("Gói cước không tồn tại");

            entity.PlanName = request.PlanName;
            entity.Description = request.Description;
            entity.DurationDays = request.DurationDays;
            entity.Price = request.Price;
            entity.IsActive = request.IsActive;
            entity.Features = request.Features;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return new SubscriptionPlanDTO
            {
                Id = entity.Id,
                PlanName = entity.PlanName,
                Description = entity.Description,
                DurationDays = entity.DurationDays,
                Price = entity.Price,
                IsActive = entity.IsActive,
                Features = entity.Features,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            var entity = await repo.FindByIdAsync(id);

            if (entity == null)
                return false;

            // Soft delete by setting IsActive to false
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(string id)
        {
            var repo = _unitOfWork.GetRepository<SubscriptionPlan>();
            var entity = await repo.FindByIdAsync(id);

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
