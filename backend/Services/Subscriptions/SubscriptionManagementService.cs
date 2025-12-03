using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Subscriptions;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.DTOs.Subscriptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Services.Subscriptions
{
    /// <summary>
    /// Service quản lý subscription của user (sử dụng FK thay vì Navigation Properties)
    /// </summary>
    public class SubscriptionManagementService : ISubscriptionManagementService
    {
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<MemberSubscription> _memberSubscriptionRepository;
        private readonly IRepository<SubscriptionPlan> _subscriptionPlanRepository;
        private readonly IRepository<Entities.Groups.Group> _groupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriptionManagementService> _logger;

        public SubscriptionManagementService(
            IUnitOfWork unitOfWork,
            ILogger<SubscriptionManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _membershipGroupRepository = unitOfWork.GetRepository<MembershipGroup>();
            _memberSubscriptionRepository = unitOfWork.GetRepository<MemberSubscription>();
            _subscriptionPlanRepository = unitOfWork.GetRepository<SubscriptionPlan>();
            _groupRepository = unitOfWork.GetRepository<Entities.Groups.Group>();
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách subscription của user theo UserZaloId (chỉ sử dụng FK)
        /// </summary>
        public async Task<List<UserSubscriptionDTO>> GetUserSubscriptionsByUserZaloIdAsync(string userZaloId)
        {
            try
            {
                if (string.IsNullOrEmpty(userZaloId))
                {
                    throw new CustomException("UserZaloId không được để trống");
                }

                // Lấy danh sách MembershipGroup của user (sử dụng FK: UserZaloId)
                var membershipGroups = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .ToListAsync();

                if (membershipGroups == null || !membershipGroups.Any())
                {
                    _logger.LogInformation("User {UserZaloId} has no approved membership groups", userZaloId);
                    return new List<UserSubscriptionDTO>();
                }

                var result = new List<UserSubscriptionDTO>();

                foreach (var mg in membershipGroups)
                {
                    // Lấy subscriptions theo MembershipGroupId (FK)
                    var subscriptions = await _memberSubscriptionRepository.AsQueryable()
                        .Where(s => s.MembershipGroupId == mg.Id)
                        .ToListAsync();

                    foreach (var sub in subscriptions)
                    {
                        // Lấy plan details theo SubscriptionPlanId (FK)
                        var plan = await _subscriptionPlanRepository
                            .GetFirstOrDefaultAsync(p => p.Id == sub.SubscriptionPlanId);

                        if (plan != null)
                        {
                            // Lấy Group name theo GroupId (FK)
                            var group = await _groupRepository
                                .GetFirstOrDefaultAsync(g => g.Id == mg.GroupId);

                            result.Add(new UserSubscriptionDTO
                            {
                                Id = sub.Id,
                                MembershipGroupId = sub.MembershipGroupId,
                                SubscriptionPlanId = sub.SubscriptionPlanId,
                                PlanName = plan.PlanName,
                                DurationDays = plan.DurationDays,
                                Price = plan.Price,
                                StartDate = sub.StartDate,
                                EndDate = sub.EndDate,
                                IsActive = sub.IsActive,
                                Notes = sub.Notes,
                                GroupName = group?.GroupName ?? "N/A",
                                CreatedDate = sub.CreatedDate
                            });
                        }
                    }
                }

                return result.OrderByDescending(s => s.CreatedDate).ToList();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user subscriptions for UserZaloId: {UserZaloId}", userZaloId);
                throw new CustomException("Có lỗi xảy ra khi tải danh sách gói cước");
            }
        }

        /// <summary>
        /// Thêm subscription mới cho user (chỉ sử dụng FK)
        /// </summary>
        public async Task<MemberSubscriptionDTO> AddUserSubscriptionAsync(
            string userZaloId,
            string subscriptionPlanId,
            DateTime? startDate,
            int? additionalDays,
            string? notes)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(userZaloId))
                {
                    throw new CustomException("UserZaloId không được để trống");
                }

                if (string.IsNullOrEmpty(subscriptionPlanId))
                {
                    throw new CustomException("Vui lòng chọn gói cước");
                }

                // Lấy MembershipGroup của user (sử dụng FK: UserZaloId) - lấy cái đã được duyệt
                var membershipGroups = await _membershipGroupRepository.AsQueryable()
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .ToListAsync();

                var approvedMembershipGroup = membershipGroups.FirstOrDefault();

                if (approvedMembershipGroup == null)
                {
                    throw new CustomException("Thành viên chưa được duyệt vào hội nào");
                }

                // Lấy plan details để tính end date (sử dụng FK: SubscriptionPlanId)
                var plan = await _subscriptionPlanRepository
                    .GetFirstOrDefaultAsync(p => p.Id == subscriptionPlanId);

                if (plan == null)
                {
                    throw new CustomException("Gói cước không tồn tại");
                }

                if (!plan.IsActive)
                {
                    throw new CustomException("Gói cước này không còn hoạt động");
                }

                // Tính end date
                var effectiveStartDate = startDate ?? DateTime.Now;
                var totalDays = plan.DurationDays + (additionalDays ?? 0);
                var endDate = effectiveStartDate.AddDays(totalDays);

                // Tạo subscription entity (chỉ sử dụng FK, không dùng Navigation)
                var subscription = new MemberSubscription
                {
                    MembershipGroupId = approvedMembershipGroup.Id,  // FK
                    SubscriptionPlanId = subscriptionPlanId,          // FK
                    StartDate = effectiveStartDate,
                    EndDate = endDate,
                    IsActive = true,
                    Notes = notes ?? "Thêm thủ công từ CMS"
                };

                await _memberSubscriptionRepository.AddAsync(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Added subscription {SubscriptionId} for user {UserZaloId}, plan {PlanId}, duration {Days} days",
                    subscription.Id, userZaloId, subscriptionPlanId, totalDays);

                // Return DTO
                return new MemberSubscriptionDTO
                {
                    Id = subscription.Id,
                    MembershipGroupId = subscription.MembershipGroupId,
                    SubscriptionPlanId = subscription.SubscriptionPlanId,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    IsActive = subscription.IsActive,
                    Notes = subscription.Notes,
                    CreatedDate = subscription.CreatedDate
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user subscription for UserZaloId: {UserZaloId}", userZaloId);
                throw new CustomException("Có lỗi xảy ra khi thêm gói cước");
            }
        }

        /// <summary>
        /// Cập nhật/gia hạn subscription (chỉ sử dụng FK)
        /// </summary>
        public async Task UpdateUserSubscriptionAsync(
            string subscriptionId,
            int? additionalDays,
            string? notes,
            bool? isActive)
        {
            try
            {
                if (string.IsNullOrEmpty(subscriptionId))
                {
                    throw new CustomException("SubscriptionId không được để trống");
                }

                // Lấy subscription hiện tại (chỉ dùng FK)
                var subscription = await _memberSubscriptionRepository
                    .GetFirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription == null)
                {
                    throw new CustomException("Không tìm thấy gói cước");
                }

                // Cập nhật thông tin
                if (additionalDays.HasValue && additionalDays > 0)
                {
                    subscription.EndDate = subscription.EndDate.AddDays(additionalDays.Value);
                    _logger.LogInformation(
                        "Extended subscription {SubscriptionId} by {Days} days, new end date: {EndDate}",
                        subscriptionId, additionalDays.Value, subscription.EndDate);
                }

                if (!string.IsNullOrEmpty(notes))
                {
                    subscription.Notes = notes;
                }

                if (isActive.HasValue)
                {
                    subscription.IsActive = isActive.Value;
                    _logger.LogInformation(
                        "Updated subscription {SubscriptionId} active status to {IsActive}",
                        subscriptionId, isActive.Value);
                }

                subscription.UpdatedDate = DateTime.Now;

                _memberSubscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated subscription {SubscriptionId} successfully", subscriptionId);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
                throw new CustomException("Có lỗi xảy ra khi cập nhật gói cước");
            }
        }
    }
}

