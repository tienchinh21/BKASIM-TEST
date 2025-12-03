using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Subscriptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Subscriptions;

namespace MiniAppGIBA.Services.Subscriptions
{
    public class GroupPackageConfigService : Service<GroupPackageConfig>, IGroupPackageConfigService
    {
        private readonly ILogger<GroupPackageConfigService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GroupPackageConfigService(IUnitOfWork unitOfWork, ILogger<GroupPackageConfigService> logger) : base(unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GroupPackageConfigDTO>> GetPagedAsync(int page, int pageSize, string keyword)
        {
            var configRepo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            var query = configRepo.AsQueryable().Where(c => c.IsActive);

            // Filter by keyword (search in group name or plan name)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var groupIds = await groupRepo.AsQueryable()
                    .Where(g => g.GroupName.Contains(keyword))
                    .Select(g => g.Id)
                    .ToListAsync();

                var planIds = await planRepo.AsQueryable()
                    .Where(p => p.PlanName.Contains(keyword))
                    .Select(p => p.Id)
                    .ToListAsync();

                query = query.Where(c => groupIds.Contains(c.GroupId) || planIds.Contains(c.SubscriptionPlanId));
            }

            var totalRecords = await query.CountAsync();
            var configs = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<GroupPackageConfigDTO>();
            foreach (var config in configs)
            {
                var group = await groupRepo.FindByIdAsync(config.GroupId);
                var plan = await planRepo.FindByIdAsync(config.SubscriptionPlanId);

                result.Add(new GroupPackageConfigDTO
                {
                    Id = config.Id,
                    GroupId = config.GroupId,
                    GroupName = group?.GroupName ?? "N/A",
                    SubscriptionPlanId = config.SubscriptionPlanId,
                    PlanName = plan?.PlanName ?? "N/A",
                    IsActive = config.IsActive,
                    CreatedDate = config.CreatedDate,
                    UpdatedDate = config.UpdatedDate
                });
            }

            return new PagedResult<GroupPackageConfigDTO>
            {
                Items = result,
                TotalItems = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<GroupPackageConfigDTO> CreateAsync(CreateGroupPackageConfigDTO request)
        {
            var configRepo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            // Check if group exists
            var group = await groupRepo.FindByIdAsync(request.GroupId);
            if (group == null)
                throw new ArgumentException("Nhóm không tồn tại");

            // Check if plan exists
            var plan = await planRepo.FindByIdAsync(request.SubscriptionPlanId);
            if (plan == null)
                throw new ArgumentException("Gói cước không tồn tại");

            // Check if config already exists
            var existing = await configRepo.AsQueryable()
                .FirstOrDefaultAsync(c => c.GroupId == request.GroupId && c.SubscriptionPlanId == request.SubscriptionPlanId);

            if (existing != null)
                throw new ArgumentException("Gói cước đã được gán cho nhóm này");

            var entity = new GroupPackageConfig
            {
                GroupId = request.GroupId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                IsActive = request.IsActive
            };

            await configRepo.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new GroupPackageConfigDTO
            {
                Id = entity.Id,
                GroupId = entity.GroupId,
                GroupName = group.GroupName,
                SubscriptionPlanId = entity.SubscriptionPlanId,
                PlanName = plan.PlanName,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var repo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var entity = await repo.FindByIdAsync(id);

            if (entity == null)
                return false;

            // Soft delete
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(string id)
        {
            var repo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var entity = await repo.FindByIdAsync(id);

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<GroupPackageConfigDTO?> GetByIdAsync(string id)
        {
            var configRepo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            var config = await configRepo.FindByIdAsync(id);
            if (config == null)
                return null;

            var group = await groupRepo.FindByIdAsync(config.GroupId);
            var plan = await planRepo.FindByIdAsync(config.SubscriptionPlanId);

            return new GroupPackageConfigDTO
            {
                Id = config.Id,
                GroupId = config.GroupId,
                GroupName = group?.GroupName ?? "N/A",
                SubscriptionPlanId = config.SubscriptionPlanId,
                PlanName = plan?.PlanName ?? "N/A",
                IsActive = config.IsActive,
                CreatedDate = config.CreatedDate,
                UpdatedDate = config.UpdatedDate
            };
        }

        public async Task<GroupPackageConfigDTO?> UpdateAsync(string id, UpdateGroupPackageConfigDTO request)
        {
            var configRepo = _unitOfWork.GetRepository<GroupPackageConfig>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            var config = await configRepo.FindByIdAsync(id);
            if (config == null)
                return null;

            // Verify group exists
            var group = await groupRepo.FindByIdAsync(request.GroupId);
            if (group == null)
                throw new ArgumentException("Nhóm không tồn tại");

            // Verify plan exists
            var plan = await planRepo.FindByIdAsync(request.SubscriptionPlanId);
            if (plan == null)
                throw new ArgumentException("Gói cước không tồn tại");

            // Check if another config with same group+plan exists
            var existing = await configRepo.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id != id && c.GroupId == request.GroupId && c.SubscriptionPlanId == request.SubscriptionPlanId);

            if (existing != null)
                throw new ArgumentException("Gói cước đã được gán cho nhóm này");

            config.GroupId = request.GroupId;
            config.SubscriptionPlanId = request.SubscriptionPlanId;
            config.IsActive = request.IsActive;
            config.UpdatedDate = DateTime.Now;

            configRepo.Update(config);
            await _unitOfWork.SaveChangesAsync();

            return new GroupPackageConfigDTO
            {
                Id = config.Id,
                GroupId = config.GroupId,
                GroupName = group.GroupName,
                SubscriptionPlanId = config.SubscriptionPlanId,
                PlanName = plan.PlanName,
                IsActive = config.IsActive,
                CreatedDate = config.CreatedDate,
                UpdatedDate = config.UpdatedDate
            };
        }
    }
}

