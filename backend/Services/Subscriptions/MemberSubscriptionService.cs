using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Subscriptions;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Services.Subscriptions
{
    public class MemberSubscriptionService : IMemberSubscriptionService
    {
        protected readonly IUnitOfWork _unitOfWork;

        public MemberSubscriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MemberSubscriptionDTO>> GetByMembershipGroupIdAsync(string membershipGroupId)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
            var membershipRepo = _unitOfWork.GetRepository<Membership>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            return await repo.AsQueryable()
                .Where(x => x.MembershipGroupId == membershipGroupId)
                .Join(membershipGroupRepo.AsQueryable(),
                    sub => sub.MembershipGroupId,
                    mg => mg.Id,
                    (sub, mg) => new { sub, mg })
                .Join(membershipRepo.AsQueryable(),
                    x => x.mg.UserZaloId,
                    m => m.UserZaloId,
                    (x, m) => new { x.sub, x.mg, m })
                .Join(groupRepo.AsQueryable(),
                    x => x.mg.GroupId,
                    g => g.Id,
                    (x, g) => new { x.sub, x.mg, x.m, g })
                .Join(planRepo.AsQueryable(),
                    x => x.sub.SubscriptionPlanId,
                    p => p.Id,
                    (x, p) => new MemberSubscriptionDTO
                    {
                        Id = x.sub.Id,
                        MembershipGroupId = x.sub.MembershipGroupId,
                        UserZaloId = x.m.UserZaloId,
                        UserName = x.m.Fullname ?? "N/A",
                        GroupName = x.g.GroupName,
                        SubscriptionPlanId = x.sub.SubscriptionPlanId,
                        PlanName = p.PlanName,
                        StartDate = x.sub.StartDate,
                        EndDate = x.sub.EndDate,
                        IsActive = x.sub.IsActive,
                        Notes = x.sub.Notes,
                        CreatedDate = x.sub.CreatedDate,
                        UpdatedDate = x.sub.UpdatedDate
                    })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<MemberSubscriptionDTO>> GetActiveByMembershipGroupIdAsync(string membershipGroupId)
        {
            var subscriptions = await GetByMembershipGroupIdAsync(membershipGroupId);
            return subscriptions.Where(x => x.IsActive && x.EndDate > DateTime.Now).ToList();
        }

        public async Task<MemberSubscriptionDTO?> GetCurrentSubscriptionAsync(string membershipGroupId)
        {
            var subscriptions = await GetActiveByMembershipGroupIdAsync(membershipGroupId);
            return subscriptions.OrderByDescending(x => x.StartDate).FirstOrDefault();
        }

        public async Task<MemberSubscriptionDTO> CreateAsync(CreateMemberSubscriptionDTO request)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            // Validate membership group exists (dùng FindByIdAsync cho string Id)
            var membershipGroup = await membershipGroupRepo.FindByIdAsync(request.MembershipGroupId);
            if (membershipGroup == null)
                throw new ArgumentException("Thành viên nhóm không tồn tại");

            // Validate plan exists (dùng FindByIdAsync cho string Id)
            var plan = await planRepo.FindByIdAsync(request.SubscriptionPlanId);
            if (plan == null)
                throw new ArgumentException("Gói cước không tồn tại");

            // Calculate end date if not provided
            var endDate = request.EndDate ?? request.StartDate.AddDays(plan.DurationDays);

            var entity = new MemberSubscription
            {
                MembershipGroupId = request.MembershipGroupId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                StartDate = request.StartDate,
                EndDate = endDate,
                IsActive = request.IsActive,
                Notes = request.Notes
            };

            await repo.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Get the created entity with related data
            var subscriptions = await GetByMembershipGroupIdAsync(request.MembershipGroupId);
            return subscriptions.First(x => x.Id == entity.Id);
        }

        public async Task<MemberSubscriptionDTO> ExtendSubscriptionAsync(string membershipGroupId, string planId, int additionalDays, DateTime? customStartDate = null)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            // Get current active subscription
            var currentSub = await repo.AsQueryable()
                .FirstOrDefaultAsync(x => x.MembershipGroupId == membershipGroupId &&
                                        x.IsActive &&
                                        x.EndDate > DateTime.Now);

            var plan = await planRepo.FindByIdAsync(planId);
            if (plan == null)
                throw new ArgumentException("Gói cước không tồn tại");

            var startDate = customStartDate ?? DateTime.Now;
            var endDate = startDate.AddDays(plan.DurationDays + additionalDays);

            var entity = new MemberSubscription
            {
                MembershipGroupId = membershipGroupId,
                SubscriptionPlanId = planId,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                Notes = $"Gia hạn từ gói cũ {(currentSub != null ? $"({currentSub.Id})" : "")}"
            };

            // Deactivate old subscription if exists
            if (currentSub != null)
            {
                currentSub.IsActive = false;
                currentSub.UpdatedDate = DateTime.Now;
                repo.Update(currentSub);
            }

            await repo.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Get the created entity with related data
            var subscriptions = await GetByMembershipGroupIdAsync(membershipGroupId);
            return subscriptions.First(x => x.Id == entity.Id);
        }

        public async Task<bool> ToggleActiveAsync(string id)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var entity = await repo.GetByIdAsync(long.Parse(id));

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedDate = DateTime.Now;

            repo.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsExpiredAsync(string membershipGroupId)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var activeSubscriptions = await repo.AsQueryable()
                .Where(x => x.MembershipGroupId == membershipGroupId &&
                           x.IsActive)
                .ToListAsync();

            return !activeSubscriptions.Any(x => x.EndDate > DateTime.Now);
        }

        public async Task<List<MemberSubscriptionDTO>> GetExpiringSoonAsync(int daysBeforeExpiry = 7)
        {
            var repo = _unitOfWork.GetRepository<MemberSubscription>();
            var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
            var membershipRepo = _unitOfWork.GetRepository<Membership>();
            var groupRepo = _unitOfWork.GetRepository<Group>();
            var planRepo = _unitOfWork.GetRepository<SubscriptionPlan>();

            var expiryDate = DateTime.Now.AddDays(daysBeforeExpiry);

            return await repo.AsQueryable()
                .Where(x => x.IsActive &&
                           x.EndDate <= expiryDate &&
                           x.EndDate > DateTime.Now)
                .Join(membershipGroupRepo.AsQueryable(),
                    sub => sub.MembershipGroupId,
                    mg => mg.Id,
                    (sub, mg) => new { sub, mg })
                .Join(membershipRepo.AsQueryable(),
                    x => x.mg.UserZaloId,
                    m => m.UserZaloId,
                    (x, m) => new { x.sub, x.mg, m })
                .Join(groupRepo.AsQueryable(),
                    x => x.mg.GroupId,
                    g => g.Id,
                    (x, g) => new { x.sub, x.mg, x.m, g })
                .Join(planRepo.AsQueryable(),
                    x => x.sub.SubscriptionPlanId,
                    p => p.Id,
                    (x, p) => new MemberSubscriptionDTO
                    {
                        Id = x.sub.Id,
                        MembershipGroupId = x.sub.MembershipGroupId,
                        UserZaloId = x.m.UserZaloId,
                        UserName = x.m.Fullname ?? "N/A",
                        GroupName = x.g.GroupName,
                        SubscriptionPlanId = x.sub.SubscriptionPlanId,
                        PlanName = p.PlanName,
                        StartDate = x.sub.StartDate,
                        EndDate = x.sub.EndDate,
                        IsActive = x.sub.IsActive,
                        Notes = x.sub.Notes,
                        CreatedDate = x.sub.CreatedDate,
                        UpdatedDate = x.sub.UpdatedDate
                    })
                .OrderBy(x => x.EndDate)
                .ToListAsync();
        }
    }
}
