using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Dashboard;

namespace MiniAppGIBA.Services.Dashboard.GroupDashboard
{
    public class GroupDashboardService : IGroupDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GroupDashboardService> _logger;

        public GroupDashboardService(
            IUnitOfWork unitOfWork,
            ILogger<GroupDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<UserRefListDTO>> GetGroupLeaderboardAsync(string groupId, string period = "month", int limit = 20, string sortBy = "TotalRefs")
        {
            try
            {
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var groupRepo = _unitOfWork.GetRepository<Group>();
                var now = DateTime.Now;
                var startDate = period.ToLower() switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    "year" => now.AddYears(-1),
                    "all" => DateTime.MinValue,
                    _ => now.AddMonths(-1)
                };

                // Get approved members of the group using FK
                var groupMembers = await membershipGroupRepo.AsQueryable()
                    .Where(mg => mg.GroupId == groupId && mg.IsApproved == true)
                    .Select(mg => mg.UserZaloId)
                    .ToListAsync();

                if (!groupMembers.Any())
                {
                    return new List<UserRefListDTO>();
                }

                // Get refs given by group members in the specified period
                var refsInPeriod = await refRepo.AsQueryable()
                    .Where(r => groupMembers.Contains(r.RefFrom) && r.CreatedDate >= startDate)
                    .ToListAsync();

                // Group by user and calculate stats
                var userStats = refsInPeriod
                    .Where(r => !string.IsNullOrEmpty(r.RefFrom))
                    .GroupBy(r => r.RefFrom)
                    .Select(g => new
                    {
                        UserZaloId = g.Key,
                        TotalRefs = g.Count(),
                        CompletedRefs = g.Count(r => r.Status == 3),
                        TotalValue = g.Sum(r => r.Value),
                        LastRefDate = g.Max(r => r.CreatedDate)
                    })
                    .ToList();

                // Apply sorting based on the sort parameter
                var sortedUserStats = sortBy.ToLower() switch
                {
                    "totalvalue" => userStats.OrderByDescending(x => x.TotalValue),
                    "successrate" => userStats.OrderByDescending(x => x.TotalRefs > 0 ? (double)x.CompletedRefs / x.TotalRefs : 0),
                    "totalrefs" or _ => userStats.OrderByDescending(x => x.TotalRefs)
                };

                // Take the limit after sorting
                var limitedUserStats = sortedUserStats.Take(limit).ToList();

                var result = new List<UserRefListDTO>();
                var ranking = 1;

                foreach (var userStat in limitedUserStats)
                {
                    // Get membership info using FK
                    var membership = await membershipRepo.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == userStat.UserZaloId);

                    if (membership != null)
                    {
                        var successRate = userStat.TotalRefs > 0
                            ? (double)userStat.CompletedRefs / userStat.TotalRefs * 100
                            : 0;

                        result.Add(new UserRefListDTO
                        {
                            UserZaloId = membership.UserZaloId,
                            Fullname = membership.Fullname,
                            PhoneNumber = membership.PhoneNumber,
                            Avatar = membership.ZaloAvatar,
                            Company = null,
                            CreatedDate = membership.CreatedDate,
                            TotalRefsGiven = userStat.TotalRefs,
                            TotalRefsReceived = 0,
                            TotalValueGiven = (decimal)userStat.TotalValue,
                            TotalValueReceived = 0,
                            SuccessRate = successRate,
                            LastRefDate = userStat.LastRefDate,
                            RefRanking = ranking,
                            ValueRanking = ranking
                        });

                        ranking++;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group leaderboard for group {GroupId}", groupId);
                throw;
            }
        }

        public async Task<GroupDashboardSummaryDTO> GetGroupDashboardSummaryAsync(string groupId)
        {
            try
            {
                var groupRepo = _unitOfWork.GetRepository<Group>();
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // Get group info using FK
                var group = await groupRepo.AsQueryable()
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                {
                    throw new ArgumentException($"Group with ID {groupId} not found");
                }

                // Get group members using FK
                var groupMembers = await membershipGroupRepo.AsQueryable()
                    .Where(mg => mg.GroupId == groupId && mg.IsApproved == true)
                    .Select(mg => mg.UserZaloId)
                    .ToListAsync();

                var totalMembers = groupMembers.Count;
                var activeMembers = groupMembers.Count; // All approved members are considered active

                // Get refs given by group members
                var allRefs = await refRepo.AsQueryable()
                    .Where(r => groupMembers.Contains(r.RefFrom))
                    .ToListAsync();

                var refsThisMonth = allRefs.Count(r => r.CreatedDate >= startOfMonth);
                var refsLastMonth = allRefs.Count(r => r.CreatedDate >= startOfLastMonth && r.CreatedDate < startOfMonth);

                var totalRefsGiven = allRefs.Count;
                var completedRefsGiven = allRefs.Count(r => r.Status == 3);
                var totalValueGiven = allRefs.Sum(r => r.Value);
                var successRate = totalRefsGiven > 0 ? (double)completedRefsGiven / totalRefsGiven * 100 : 0;

                var growthRate = refsLastMonth > 0
                    ? ((double)(refsThisMonth - refsLastMonth) / refsLastMonth) * 100
                    : 0;

                // Get top performer
                var topPerformer = allRefs
                    .GroupBy(r => r.RefFrom)
                    .Select(g => new
                    {
                        UserZaloId = g.Key,
                        RefCount = g.Count()
                    })
                    .OrderByDescending(x => x.RefCount)
                    .FirstOrDefault();

                string? topPerformerName = null;
                int topPerformerRefs = 0;

                if (topPerformer != null)
                {
                    var topMember = await membershipRepo.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == topPerformer.UserZaloId);
                    topPerformerName = topMember?.Fullname;
                    topPerformerRefs = topPerformer.RefCount;
                }

                return new GroupDashboardSummaryDTO
                {
                    GroupId = group.Id,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    IsActive = group.IsActive,
                    TotalMembers = totalMembers,
                    ActiveMembers = activeMembers,
                    TotalRefsGiven = totalRefsGiven,
                    CompletedRefsGiven = completedRefsGiven,
                    TotalValueGiven = (decimal)totalValueGiven,
                    SuccessRate = successRate,
                    RefsThisMonth = refsThisMonth,
                    RefsLastMonth = refsLastMonth,
                    GrowthRate = growthRate,
                    TopPerformerName = topPerformerName,
                    TopPerformerRefs = topPerformerRefs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group dashboard summary for group {GroupId}", groupId);
                throw;
            }
        }

        public async Task<List<GroupLeaderboardSummaryDTO>> GetGroupsLeaderboardSummaryAsync(string period = "month")
        {
            try
            {
                var groupRepo = _unitOfWork.GetRepository<Group>();
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var now = DateTime.Now;
                var startDate = period.ToLower() switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    "year" => now.AddYears(-1),
                    "all" => DateTime.MinValue,
                    _ => now.AddMonths(-1)
                };

                // Get all active groups
                var groups = await groupRepo.AsQueryable()
                    .Where(g => g.IsActive)
                    .ToListAsync();

                var result = new List<GroupLeaderboardSummaryDTO>();

                foreach (var group in groups)
                {
                    // Get group members using FK
                    var groupMembers = await membershipGroupRepo.AsQueryable()
                        .Where(mg => mg.GroupId == group.Id && mg.IsApproved == true)
                        .Select(mg => mg.UserZaloId)
                        .ToListAsync();

                    if (!groupMembers.Any())
                    {
                        continue;
                    }

                    // Get refs given by group members in the specified period
                    var refsInPeriod = await refRepo.AsQueryable()
                        .Where(r => groupMembers.Contains(r.RefFrom) && r.CreatedDate >= startDate)
                        .ToListAsync();

                    var totalRefs = refsInPeriod.Count;
                    var completedRefs = refsInPeriod.Count(r => r.Status == 3);
                    var totalValue = refsInPeriod.Sum(r => r.Value);
                    var successRate = totalRefs > 0 ? (double)completedRefs / totalRefs * 100 : 0;

                    // Get top performer
                    var topPerformer = refsInPeriod
                        .GroupBy(r => r.RefFrom)
                        .Select(g => new
                        {
                            UserZaloId = g.Key,
                            RefCount = g.Count()
                        })
                        .OrderByDescending(x => x.RefCount)
                        .FirstOrDefault();

                    string? topPerformerName = null;
                    int topPerformerRefs = 0;

                    if (topPerformer != null)
                    {
                        var topMember = await membershipRepo.AsQueryable()
                            .FirstOrDefaultAsync(m => m.UserZaloId == topPerformer.UserZaloId);
                        topPerformerName = topMember?.Fullname;
                        topPerformerRefs = topPerformer.RefCount;
                    }

                    result.Add(new GroupLeaderboardSummaryDTO
                    {
                        GroupId = group.Id,
                        GroupName = group.GroupName,
                        TotalMembers = groupMembers.Count,
                        TotalRefs = totalRefs,
                        TotalValue = (decimal)totalValue,
                        SuccessRate = successRate,
                        TopPerformerName = topPerformerName,
                        TopPerformerRefs = topPerformerRefs
                    });
                }

                // Sort by total refs and assign ranking
                result = result.OrderByDescending(g => g.TotalRefs).ToList();
                for (int i = 0; i < result.Count; i++)
                {
                    result[i].Ranking = i + 1;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups leaderboard summary");
                throw;
            }
        }

        public async Task<List<UserRefListDTO>> GetGroupTop3Async(string groupId, string period = "month")
        {
            return await GetGroupLeaderboardAsync(groupId, period, 3);
        }

        public async Task<List<GroupMonthlyRefDataDTO>> GetGroupMonthlyRefDataAsync(string groupId, int months = 12)
        {
            try
            {
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var now = DateTime.Now;
                var startDate = now.AddMonths(-months);

                // Get group members using FK
                var groupMembers = await membershipGroupRepo.AsQueryable()
                    .Where(mg => mg.GroupId == groupId && mg.IsApproved == true)
                    .Select(mg => mg.UserZaloId)
                    .ToListAsync();

                if (!groupMembers.Any())
                {
                    return new List<GroupMonthlyRefDataDTO>();
                }

                // Get refs given by group members
                var refsGiven = await refRepo.AsQueryable()
                    .Where(r => groupMembers.Contains(r.RefFrom) && r.CreatedDate >= startDate)
                    .ToListAsync();

                // Get refs received by group members
                var refsReceived = await refRepo.AsQueryable()
                    .Where(r => groupMembers.Contains(r.RefTo) && r.CreatedDate >= startDate)
                    .ToListAsync();

                // Group refs given by month
                var monthlyRefsGiven = refsGiven
                    .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalRefs = g.Count(),
                        CompletedRefs = g.Count(r => r.Status == 3),
                        TotalValue = g.Sum(r => r.Value)
                    })
                    .ToList();

                // Group refs received by month
                var monthlyRefsReceived = refsReceived
                    .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        RefsReceived = g.Count()
                    })
                    .ToList();

                var result = new List<GroupMonthlyRefDataDTO>();

                // Fill in missing months with zero data
                for (int i = months - 1; i >= 0; i--)
                {
                    var date = now.AddMonths(-i);
                    var monthDataGiven = monthlyRefsGiven.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);
                    var monthDataReceived = monthlyRefsReceived.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);

                    var monthName = date.ToString("MMM yyyy", new System.Globalization.CultureInfo("vi-VN"));
                    var successRate = monthDataGiven?.TotalRefs > 0 ? (double)monthDataGiven.CompletedRefs / monthDataGiven.TotalRefs * 100 : 0;

                    result.Add(new GroupMonthlyRefDataDTO
                    {
                        Month = monthName,
                        Year = date.Year,
                        MonthNumber = date.Month,
                        TotalRefs = monthDataGiven?.TotalRefs ?? 0,
                        CompletedRefs = monthDataGiven?.CompletedRefs ?? 0,
                        TotalValue = (decimal)(monthDataGiven?.TotalValue ?? 0),
                        SuccessRate = successRate,
                        RefsReceived = monthDataReceived?.RefsReceived ?? 0,
                        ActiveMembers = groupMembers.Count, // Simplified
                        NewMembers = 0 // Would need additional logic to track new members per month
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group monthly ref data for group {GroupId}", groupId);
                throw;
            }
        }
    }
}
