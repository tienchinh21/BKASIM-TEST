using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Sponsors;
using MiniAppGIBA.Models.DTOs.Dashboard;
using MiniAppGIBA.Constants;

namespace MiniAppGIBA.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(
            IUnitOfWork unitOfWork,
            ILogger<DashboardService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardStatisticsDTO> GetDashboardStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // === GROUPS ===
                var totalGroups = await _unitOfWork.GetRepository<Group>().CountAsync();
                var activeGroups = await _unitOfWork.GetRepository<Group>().CountAsync(g => g.IsActive);
                var inactiveGroups = totalGroups - activeGroups;

                // === EVENTS ===
                var totalEvents = await _unitOfWork.GetRepository<Event>().CountAsync();
                var upcomingEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.StartTime > now && e.IsActive);
                var ongoingEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.StartTime <= now && e.EndTime >= now && e.IsActive);
                var completedEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.EndTime < now && e.IsActive);

                // === MEMBERS ===
                var totalMembers = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.IsDelete != true);
                var activeMembers = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.CreatedDate >= startOfMonth.AddMonths(-6) && m.IsDelete != true); // Active trong 6 tháng gần nhất

                // === PENDING APPROVALS ===
                // Đơn chờ duyệt vào nhóm (MembershipGroup.IsApproved = null)
                var pendingGroupApprovals = await _unitOfWork.GetRepository<MembershipGroup>()
                    .CountAsync(mg => mg.IsApproved == null);

                // Đơn chờ duyệt khách mời sự kiện (EventGuest.Status = 1 - Pending)
                var pendingEventGuestApprovals = await _unitOfWork.GetRepository<EventGuest>()
                    .CountAsync(eg => eg.Status == 1);

                // Đơn đăng ký sự kiện chờ xác nhận (EventRegistration.Status = 1 - Registered)
                var pendingEventRegistrations = await _unitOfWork.GetRepository<EventRegistration>()
                    .CountAsync(er => er.Status == 1);

                // === REFS ===
                var totalRefs = await _unitOfWork.GetRepository<Ref>().CountAsync();
                var pendingRefs = await _unitOfWork.GetRepository<Ref>().CountAsync(r => r.Status == 1); // Sent
                var completedRefs = await _unitOfWork.GetRepository<Ref>().CountAsync(r => r.Status == 3); // Completed

                // === EVENT REGISTRATIONS ===
                var totalEventRegistrations = await _unitOfWork.GetRepository<EventRegistration>().CountAsync();
                var totalEventGuests = await _unitOfWork.GetRepository<EventGuest>().CountAsync();
                var totalGuestLists = await _unitOfWork.GetRepository<GuestList>().CountAsync();

                // === SUBSCRIPTIONS ===
                var totalSubscriptionPlans = await _unitOfWork.GetRepository<Entities.Subscriptions.SubscriptionPlan>().CountAsync();
                var activeSubscriptionPlans = await _unitOfWork.GetRepository<Entities.Subscriptions.SubscriptionPlan>()
                    .CountAsync(sp => sp.IsActive);

                var totalMemberSubscriptions = await _unitOfWork.GetRepository<Entities.Subscriptions.MemberSubscription>().CountAsync();
                var activeMemberSubscriptions = await _unitOfWork.GetRepository<Entities.Subscriptions.MemberSubscription>()
                    .CountAsync(ms => ms.IsActive && ms.EndDate > now);

                // Subscription sắp hết hạn (trong 7 ngày tới)
                var expiringSoonDate = now.AddDays(7);
                var expiringSoonSubscriptions = await _unitOfWork.GetRepository<Entities.Subscriptions.MemberSubscription>()
                    .CountAsync(ms => ms.IsActive && ms.EndDate > now && ms.EndDate <= expiringSoonDate);

                // === SPONSORS ===
                var totalSponsors = await _unitOfWork.GetRepository<Sponsor>().CountAsync();
                var totalSponsorshipTiers = await _unitOfWork.GetRepository<SponsorshipTier>().CountAsync();

                // === GIFTS ===
                var totalEventGifts = await _unitOfWork.GetRepository<EventGift>().CountAsync();

                return new DashboardStatisticsDTO
                {
                    // Groups
                    TotalGroups = totalGroups,
                    ActiveGroups = activeGroups,
                    InactiveGroups = inactiveGroups,

                    // Events
                    TotalEvents = totalEvents,
                    UpcomingEvents = upcomingEvents,
                    OngoingEvents = ongoingEvents,
                    CompletedEvents = completedEvents,

                    // Members
                    TotalMembers = totalMembers,
                    ActiveMembers = activeMembers,

                    // Pending Approvals
                    PendingGroupApprovals = pendingGroupApprovals,
                    PendingEventGuestApprovals = pendingEventGuestApprovals,
                    PendingEventRegistrations = pendingEventRegistrations,

                    // Refs
                    TotalRefs = totalRefs,
                    PendingRefs = pendingRefs,
                    CompletedRefs = completedRefs,

                    // Event Registrations
                    TotalEventRegistrations = totalEventRegistrations,
                    TotalEventGuests = totalEventGuests,
                    TotalGuestLists = totalGuestLists,

                    // Subscriptions
                    TotalSubscriptionPlans = totalSubscriptionPlans,
                    ActiveSubscriptionPlans = activeSubscriptionPlans,
                    TotalMemberSubscriptions = totalMemberSubscriptions,
                    ActiveMemberSubscriptions = activeMemberSubscriptions,
                    ExpiringSoonSubscriptions = expiringSoonSubscriptions,

                    // Sponsors
                    TotalSponsors = totalSponsors,
                    TotalSponsorshipTiers = totalSponsorshipTiers,

                    // Gifts
                    TotalEventGifts = totalEventGifts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }
        }

        public async Task<GroupsStatisticsDTO> GetGroupsStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                var totalGroups = await _unitOfWork.GetRepository<Group>().CountAsync();
                var activeGroups = await _unitOfWork.GetRepository<Group>().CountAsync(g => g.IsActive);
                var inactiveGroups = totalGroups - activeGroups;

                var groupsThisMonth = await _unitOfWork.GetRepository<Group>()
                    .CountAsync(g => g.CreatedDate >= startOfMonth);
                var groupsLastMonth = await _unitOfWork.GetRepository<Group>()
                    .CountAsync(g => g.CreatedDate >= startOfLastMonth && g.CreatedDate < startOfMonth);

                var growthRate = groupsLastMonth > 0
                    ? ((double)(groupsThisMonth - groupsLastMonth) / groupsLastMonth) * 100
                    : 0;

                return new GroupsStatisticsDTO
                {
                    TotalGroups = totalGroups,
                    ActiveGroups = activeGroups,
                    InactiveGroups = inactiveGroups,
                    GroupsCreatedThisMonth = groupsThisMonth,
                    GroupsCreatedLastMonth = groupsLastMonth,
                    GroupsGrowthRate = Math.Round(growthRate, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups statistics");
                throw;
            }
        }

        public async Task<EventsStatisticsDTO> GetEventsStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                var totalEvents = await _unitOfWork.GetRepository<Event>().CountAsync();
                var upcomingEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.StartTime > now && e.IsActive);
                var ongoingEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.StartTime <= now && e.EndTime >= now && e.IsActive);
                var completedEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.EndTime < now && e.IsActive);
                var cancelledEvents = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => !e.IsActive);

                var eventsThisMonth = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.CreatedDate >= startOfMonth);
                var eventsLastMonth = await _unitOfWork.GetRepository<Event>()
                    .CountAsync(e => e.CreatedDate >= startOfLastMonth && e.CreatedDate < startOfMonth);

                var growthRate = eventsLastMonth > 0
                    ? ((double)(eventsThisMonth - eventsLastMonth) / eventsLastMonth) * 100
                    : 0;

                var totalParticipants = await _unitOfWork.GetRepository<EventRegistration>()
                    .CountAsync(er => er.Status != 3); // Exclude cancelled

                var averageParticipants = totalEvents > 0 ? totalParticipants / totalEvents : 0;

                return new EventsStatisticsDTO
                {
                    TotalEvents = totalEvents,
                    UpcomingEvents = upcomingEvents,
                    OngoingEvents = ongoingEvents,
                    CompletedEvents = completedEvents,
                    CancelledEvents = cancelledEvents,
                    EventsThisMonth = eventsThisMonth,
                    EventsLastMonth = eventsLastMonth,
                    EventsGrowthRate = Math.Round(growthRate, 2),
                    TotalParticipants = totalParticipants,
                    AverageParticipantsPerEvent = averageParticipants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events statistics");
                throw;
            }
        }

        public async Task<MembersStatisticsDTO> GetMembersStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                var totalMembers = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.IsDelete != true);
                var activeMembers = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.CreatedDate >= startOfMonth.AddMonths(-6) && m.IsDelete != true);

                var newMembersThisMonth = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.CreatedDate >= startOfMonth && m.IsDelete != true);
                var newMembersLastMonth = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.CreatedDate >= startOfLastMonth && m.CreatedDate < startOfMonth && m.IsDelete != true);

                var growthRate = newMembersLastMonth > 0
                    ? ((double)(newMembersThisMonth - newMembersLastMonth) / newMembersLastMonth) * 100
                    : 0;

                // Profile and Address fields removed from Membership - count all non-deleted members
                var membersWithProfile = await _unitOfWork.GetRepository<Membership>()
                    .CountAsync(m => m.IsDelete != true);

                return new MembersStatisticsDTO
                {
                    TotalMembers = totalMembers,
                    ActiveMembers = activeMembers,
                    InactiveMembers = totalMembers - activeMembers,
                    NewMembersThisMonth = newMembersThisMonth,
                    NewMembersLastMonth = newMembersLastMonth,
                    MembersGrowthRate = Math.Round(growthRate, 2),
                    MembersWithProfile = membersWithProfile,
                    MembersWithoutProfile = totalMembers - membersWithProfile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members statistics");
                throw;
            }
        }

        public async Task<RefsStatisticsDTO> GetRefsStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                var totalRefs = await _unitOfWork.GetRepository<Ref>().CountAsync();
                var pendingRefs = await _unitOfWork.GetRepository<Ref>().CountAsync(r => r.Status == 1);
                var completedRefs = await _unitOfWork.GetRepository<Ref>().CountAsync(r => r.Status == 2);
                var cancelledRefs = await _unitOfWork.GetRepository<Ref>().CountAsync(r => r.Status == 3);

                var refsThisMonth = await _unitOfWork.GetRepository<Ref>()
                    .CountAsync(r => r.CreatedDate >= startOfMonth);
                var refsLastMonth = await _unitOfWork.GetRepository<Ref>()
                    .CountAsync(r => r.CreatedDate >= startOfLastMonth && r.CreatedDate < startOfMonth);

                var growthRate = refsLastMonth > 0
                    ? ((double)(refsThisMonth - refsLastMonth) / refsLastMonth) * 100
                    : 0;

                var totalRefValue = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.Status == 2 && r.Value > 0)
                    .SumAsync(r => r.Value);

                var averageRefValue = completedRefs > 0 ? totalRefValue / completedRefs : 0;

                return new RefsStatisticsDTO
                {
                    TotalRefs = totalRefs,
                    PendingRefs = pendingRefs,
                    CompletedRefs = completedRefs,
                    CancelledRefs = cancelledRefs,
                    RefsThisMonth = refsThisMonth,
                    RefsLastMonth = refsLastMonth,
                    RefsGrowthRate = Math.Round(growthRate, 2),
                    TotalRefValue = (decimal)totalRefValue,
                    AverageRefValue = Math.Round((decimal)averageRefValue, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs statistics");
                throw;
            }
        }

        public async Task<List<UpcomingEventDTO>> GetUpcomingEventsAsync(int limit = 5)
        {
            try
            {
                var now = DateTime.Now;
                var eventRepo = _unitOfWork.GetRepository<Event>();
                var groupRepo = _unitOfWork.GetRepository<Group>();
                var registrationRepo = _unitOfWork.GetRepository<EventRegistration>();

                // Query database - chỉ lấy raw data (không gọi method)
                var eventsQuery = await eventRepo.AsQueryable()
                    .Where(e => e.StartTime > now && e.IsActive)
                    .OrderBy(e => e.StartTime)
                    .Take(limit)
                    .ToListAsync();

                // Process trên client-side
                var events = new List<UpcomingEventDTO>();
                foreach (var e in eventsQuery)
                {
                    // Lấy Group bằng FK
                    var group = await groupRepo.FindByIdAsync(e.GroupId);

                    // Đếm registrations bằng FK
                    var registeredCount = await registrationRepo.AsQueryable()
                        .CountAsync(er => er.EventId == e.Id && er.Status != 3);

                    events.Add(new UpcomingEventDTO
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Banner = GetFullUrl(e.Banner), // Client-side OK
                        Address = e.Address ?? string.Empty,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        Status = GetEventStatus(e.StartTime, e.EndTime), // Client-side OK
                        StatusText = GetEventStatusText(e.StartTime, e.EndTime), // Client-side OK
                        GroupName = group?.GroupName ?? "N/A",
                        RegisteredCount = registeredCount,
                        MaxParticipants = e.JoinCount,
                        IsUnlimited = e.JoinCount == -1
                    });
                }

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming events");
                throw;
            }
        }

        public async Task<List<RecentGroupDTO>> GetRecentGroupsAsync(int limit = 5)
        {
            try
            {
                var groupRepo = _unitOfWork.GetRepository<Group>();
                var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
                var eventRepo = _unitOfWork.GetRepository<Event>();

                // Query database - chỉ lấy raw groups
                var groupsQuery = await groupRepo.AsQueryable()
                    .Where(g => g.IsActive)
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(limit)
                    .ToListAsync();

                // Process trên client-side - đếm bằng FK
                var groups = new List<RecentGroupDTO>();
                foreach (var g in groupsQuery)
                {
                    var memberCount = await membershipGroupRepo.AsQueryable()
                        .CountAsync(mg => mg.GroupId == g.Id && mg.IsApproved == true);

                    var eventCount = await eventRepo.AsQueryable()
                        .CountAsync(e => e.GroupId == g.Id && e.IsActive);

                    groups.Add(new RecentGroupDTO
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Description = g.Description ?? string.Empty,
                        IsActive = g.IsActive,
                        CreatedDate = g.CreatedDate,
                        MemberCount = memberCount,
                        EventCount = eventCount
                    });
                }

                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent groups");
                throw;
            }
        }

        public async Task<MonthlyStatisticsDTO> GetMonthlyStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var months = new List<MonthlyDataDTO>();

                // Generate last 12 months
                for (int i = 11; i >= 0; i--)
                {
                    var month = now.AddMonths(-i);
                    var startOfMonth = new DateTime(month.Year, month.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    months.Add(new MonthlyDataDTO
                    {
                        Month = month.ToString("MMM yyyy"),
                        Year = month.Year,
                        Count = 0 // Will be filled below
                    });
                }

                // Groups data
                var groupsData = await _unitOfWork.GetRepository<Group>()
                    .AsQueryable()
                    .Where(g => g.CreatedDate >= now.AddMonths(-12))
                    .GroupBy(g => new { g.CreatedDate.Year, g.CreatedDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync();

                // Events data
                var eventsData = await _unitOfWork.GetRepository<Event>()
                    .AsQueryable()
                    .Where(e => e.CreatedDate >= now.AddMonths(-12))
                    .GroupBy(e => new { e.CreatedDate.Year, e.CreatedDate.Month })
                    .Select(e => new { e.Key.Year, e.Key.Month, Count = e.Count() })
                    .ToListAsync();

                // Members data
                var membersData = await _unitOfWork.GetRepository<Membership>()
                    .AsQueryable()
                    .Where(m => m.CreatedDate >= now.AddMonths(-12) && m.IsDelete != true)
                    .GroupBy(m => new { m.CreatedDate.Year, m.CreatedDate.Month })
                    .Select(m => new { m.Key.Year, m.Key.Month, Count = m.Count() })
                    .ToListAsync();

                // Refs data
                var refsData = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.CreatedDate >= now.AddMonths(-12))
                    .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                    .Select(r => new { r.Key.Year, r.Key.Month, Count = r.Count() })
                    .ToListAsync();

                return new MonthlyStatisticsDTO
                {
                    GroupsData = months.Select(m =>
                    {
                        var monthNumber = DateTime.ParseExact(m.Month.Split(' ')[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        return new MonthlyDataDTO
                        {
                            Month = m.Month,
                            Year = m.Year,
                            Count = groupsData.FirstOrDefault(g => g.Year == m.Year && g.Month == monthNumber)?.Count ?? 0
                        };
                    }).ToList(),
                    EventsData = months.Select(m =>
                    {
                        var monthNumber = DateTime.ParseExact(m.Month.Split(' ')[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        return new MonthlyDataDTO
                        {
                            Month = m.Month,
                            Year = m.Year,
                            Count = eventsData.FirstOrDefault(e => e.Year == m.Year && e.Month == monthNumber)?.Count ?? 0
                        };
                    }).ToList(),
                    MembersData = months.Select(m =>
                    {
                        var monthNumber = DateTime.ParseExact(m.Month.Split(' ')[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        return new MonthlyDataDTO
                        {
                            Month = m.Month,
                            Year = m.Year,
                            Count = membersData.FirstOrDefault(mem => mem.Year == m.Year && mem.Month == monthNumber)?.Count ?? 0
                        };
                    }).ToList(),
                    RefsData = months.Select(m =>
                    {
                        var monthNumber = DateTime.ParseExact(m.Month.Split(' ')[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        return new MonthlyDataDTO
                        {
                            Month = m.Month,
                            Year = m.Year,
                            Count = refsData.FirstOrDefault(r => r.Year == m.Year && r.Month == monthNumber)?.Count ?? 0
                        };
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly statistics");
                throw;
            }
        }

        public async Task<List<RecentActivityDTO>> GetRecentActivitiesAsync(int limit = 10)
        {
            try
            {
                var activities = new List<RecentActivityDTO>();
                var now = DateTime.Now;

                // Recent groups
                var recentGroups = await _unitOfWork.GetRepository<Group>()
                    .AsQueryable()
                    .Where(g => g.CreatedDate >= now.AddDays(-30))
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(limit / 4)
                    .Select(g => new RecentActivityDTO
                    {
                        Id = g.Id,
                        Type = "group",
                        Title = g.GroupName,
                        Description = "Hội nhóm mới được tạo",
                        CreatedDate = g.CreatedDate,
                        UserName = "System",
                        UserAvatar = ""
                    })
                    .ToListAsync();

                // Recent events
                var recentEvents = await _unitOfWork.GetRepository<Event>()
                    .AsQueryable()
                    .Where(e => e.CreatedDate >= now.AddDays(-30))
                    .OrderByDescending(e => e.CreatedDate)
                    .Take(limit / 4)
                    .Select(e => new RecentActivityDTO
                    {
                        Id = e.Id,
                        Type = "event",
                        Title = e.Title,
                        Description = "Sự kiện mới được tạo",
                        CreatedDate = e.CreatedDate,
                        UserName = "System",
                        UserAvatar = ""
                    })
                    .ToListAsync();

                // Recent refs
                var recentRefs = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.CreatedDate >= now.AddDays(-30))
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(limit / 4)
                    .Select(r => new RecentActivityDTO
                    {
                        Id = r.Id,
                        Type = "ref",
                        Title = "Giới thiệu mới",
                        Description = r.Content,
                        CreatedDate = r.CreatedDate,
                        UserName = "System",
                        UserAvatar = ""
                    })
                    .ToListAsync();

                activities.AddRange(recentGroups);
                activities.AddRange(recentEvents);
                activities.AddRange(recentRefs);

                return activities.OrderByDescending(a => a.CreatedDate).Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                throw;
            }
        }

        private string GetFullUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return string.Empty;

            if (relativeUrl.StartsWith("http"))
                return relativeUrl;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativeUrl;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/{relativeUrl.TrimStart('/')}";
        }

        private string GetEventStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (now < startTime) return "upcoming";
            if (now >= startTime && now <= endTime) return "ongoing";
            return "completed";
        }

        private string GetEventStatusText(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (now < startTime) return "Sắp diễn ra";
            if (now >= startTime && now <= endTime) return "Đang diễn ra";
            return "Đã kết thúc";
        }


        #region User Ref Dashboard Methods

        public async Task<UserRefDashboardResultDTO> GetUserRefsListAsync(UserRefDashboardQueryDTO query)
        {
            try
            {
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var refRepo = _unitOfWork.GetRepository<Ref>();

                // Base query for memberships
                var membershipsQuery = membershipRepo.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(query.SearchKeyword))
                {
                    membershipsQuery = membershipsQuery.Where(m =>
                        m.Fullname.Contains(query.SearchKeyword) ||
                        m.PhoneNumber.Contains(query.SearchKeyword));
                }

                if (query.FromDate.HasValue)
                {
                    membershipsQuery = membershipsQuery.Where(m => m.CreatedDate >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    membershipsQuery = membershipsQuery.Where(m => m.CreatedDate <= query.ToDate.Value);
                }

                // Get total count
                var totalUsers = await membershipsQuery.CountAsync();

                // Apply sorting
                membershipsQuery = query.SortBy?.ToLower() switch
                {
                    "totalrefs" => query.SortDirection == "asc"
                        ? membershipsQuery.OrderBy(m => m.UserZaloId) // Will be calculated later
                        : membershipsQuery.OrderByDescending(m => m.UserZaloId),
                    "totalvalue" => query.SortDirection == "asc"
                        ? membershipsQuery.OrderBy(m => m.UserZaloId)
                        : membershipsQuery.OrderByDescending(m => m.UserZaloId),
                    "successrate" => query.SortDirection == "asc"
                        ? membershipsQuery.OrderBy(m => m.UserZaloId)
                        : membershipsQuery.OrderByDescending(m => m.UserZaloId),
                    _ => membershipsQuery.OrderByDescending(m => m.CreatedDate)
                };

                // Apply pagination
                var memberships = await membershipsQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                // Get ref statistics for each user
                var userRefs = new List<UserRefListDTO>();
                foreach (var membership in memberships)
                {
                    // Get refs given by this user
                    var refsGiven = await refRepo.AsQueryable()
                        .Where(r => r.RefFrom == membership.UserZaloId)
                        .ToListAsync();

                    // Get refs received by this user
                    var refsReceived = await refRepo.AsQueryable()
                        .Where(r => r.RefTo == membership.UserZaloId)
                        .ToListAsync();

                    var totalRefsGiven = refsGiven.Count;
                    var completedRefsGiven = refsGiven.Count(r => r.Status == 3); // 3 = Completed
                    var totalRefsReceived = refsReceived.Count;
                    var completedRefsReceived = refsReceived.Count(r => r.Status == 3); // 3 = Completed
                    var totalValueGiven = (decimal)refsGiven.Sum(r => r.Value);
                    var totalValueReceived = (decimal)refsReceived.Sum(r => r.Value);
                    var successRate = totalRefsGiven > 0 ? (double)completedRefsGiven / totalRefsGiven * 100 : 0;
                    var lastRefDate = refsGiven.Concat(refsReceived)
                        .OrderByDescending(r => r.CreatedDate)
                        .FirstOrDefault()?.CreatedDate;

                    userRefs.Add(new UserRefListDTO
                    {
                        UserZaloId = membership.UserZaloId,
                        Fullname = membership.Fullname,
                        PhoneNumber = membership.PhoneNumber,
                        Avatar = membership.ZaloAvatar,
                        Company = null,
                        CreatedDate = membership.CreatedDate,
                        TotalRefsGiven = totalRefsGiven,
                        TotalRefsReceived = totalRefsReceived,
                        TotalValueGiven = totalValueGiven,
                        TotalValueReceived = totalValueReceived,
                        SuccessRate = successRate,
                        LastRefDate = lastRefDate
                    });
                }

                // Re-sort based on calculated values
                userRefs = query.SortBy?.ToLower() switch
                {
                    "totalrefs" => query.SortDirection == "asc"
                        ? userRefs.OrderBy(u => u.TotalRefsGiven + u.TotalRefsReceived).ToList()
                        : userRefs.OrderByDescending(u => u.TotalRefsGiven + u.TotalRefsReceived).ToList(),
                    "totalvalue" => query.SortDirection == "asc"
                        ? userRefs.OrderBy(u => u.TotalValueGiven + u.TotalValueReceived).ToList()
                        : userRefs.OrderByDescending(u => u.TotalValueGiven + u.TotalValueReceived).ToList(),
                    "successrate" => query.SortDirection == "asc"
                        ? userRefs.OrderBy(u => u.SuccessRate).ToList()
                        : userRefs.OrderByDescending(u => u.SuccessRate).ToList(),
                    _ => userRefs.OrderByDescending(u => u.LastRefDate).ToList()
                };

                // Calculate overall summary
                var allRefs = await refRepo.AsQueryable().ToListAsync();
                var overallSummary = new UserRefSummaryDTO
                {
                    TotalRefsGiven = allRefs.Count(r => !string.IsNullOrEmpty(r.RefFrom)),
                    CompletedRefsGiven = allRefs.Count(r => !string.IsNullOrEmpty(r.RefFrom) && r.Status == 3),
                    PendingRefsGiven = allRefs.Count(r => !string.IsNullOrEmpty(r.RefFrom) && r.Status == 1),
                    CancelledRefsGiven = allRefs.Count(r => !string.IsNullOrEmpty(r.RefFrom) && r.Status == 2),
                    TotalValueGiven = (decimal)allRefs.Where(r => !string.IsNullOrEmpty(r.RefFrom)).Sum(r => r.Value),
                    TotalRefsReceived = allRefs.Count(r => !string.IsNullOrEmpty(r.RefTo)),
                    CompletedRefsReceived = allRefs.Count(r => !string.IsNullOrEmpty(r.RefTo) && r.Status == 3),
                    PendingRefsReceived = allRefs.Count(r => !string.IsNullOrEmpty(r.RefTo) && r.Status == 1),
                    CancelledRefsReceived = allRefs.Count(r => !string.IsNullOrEmpty(r.RefTo) && r.Status == 2),
                    TotalValueReceived = (decimal)allRefs.Where(r => !string.IsNullOrEmpty(r.RefTo)).Sum(r => r.Value)
                };

                overallSummary.AverageValueGiven = overallSummary.TotalRefsGiven > 0
                    ? overallSummary.TotalValueGiven / overallSummary.TotalRefsGiven
                    : 0;
                overallSummary.AverageValueReceived = overallSummary.TotalRefsReceived > 0
                    ? overallSummary.TotalValueReceived / overallSummary.TotalRefsReceived
                    : 0;
                overallSummary.SuccessRateGiven = overallSummary.TotalRefsGiven > 0
                    ? (double)overallSummary.CompletedRefsGiven / overallSummary.TotalRefsGiven * 100
                    : 0;
                overallSummary.SuccessRateReceived = overallSummary.TotalRefsReceived > 0
                    ? (double)overallSummary.CompletedRefsReceived / overallSummary.TotalRefsReceived * 100
                    : 0;

                return new UserRefDashboardResultDTO
                {
                    Users = userRefs,
                    TotalUsers = totalUsers,
                    TotalPages = (int)Math.Ceiling((double)totalUsers / query.PageSize),
                    CurrentPage = query.Page,
                    PageSize = query.PageSize,
                    OverallSummary = overallSummary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user refs list");
                throw;
            }
        }

        public async Task<UserRefDashboardDTO> GetUserRefDashboardAsync(string userZaloId)
        {
            try
            {
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var refRepo = _unitOfWork.GetRepository<Ref>();

                // Get user info
                var membership = await membershipRepo.AsQueryable()
                    .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId);

                if (membership == null)
                {
                    throw new ArgumentException($"User with ZaloId {userZaloId} not found");
                }

                // Get refs given by this user
                var refsGiven = await refRepo.AsQueryable()
                    .Where(r => r.RefFrom == userZaloId)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                // Get refs received by this user
                var refsReceived = await refRepo.AsQueryable()
                    .Where(r => r.RefTo == userZaloId)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                // Calculate summary
                var summary = new UserRefSummaryDTO
                {
                    TotalRefsGiven = refsGiven.Count,
                    CompletedRefsGiven = refsGiven.Count(r => r.Status == 3), // 3 = Completed
                    PendingRefsGiven = refsGiven.Count(r => r.Status == 1), // 1 = Sent (pending)
                    CancelledRefsGiven = refsGiven.Count(r => r.Status == 2), // 2 = Received (not completed)
                    TotalValueGiven = (decimal)refsGiven.Sum(r => r.Value),
                    TotalRefsReceived = refsReceived.Count,
                    CompletedRefsReceived = refsReceived.Count(r => r.Status == 3), // 3 = Completed
                    PendingRefsReceived = refsReceived.Count(r => r.Status == 1), // 1 = Sent (pending)
                    CancelledRefsReceived = refsReceived.Count(r => r.Status == 2), // 2 = Received (not completed)
                    TotalValueReceived = (decimal)refsReceived.Sum(r => r.Value)
                };

                summary.AverageValueGiven = summary.TotalRefsGiven > 0
                    ? summary.TotalValueGiven / summary.TotalRefsGiven
                    : 0;
                summary.AverageValueReceived = summary.TotalRefsReceived > 0
                    ? summary.TotalValueReceived / summary.TotalRefsReceived
                    : 0;
                summary.SuccessRateGiven = summary.TotalRefsGiven > 0
                    ? (double)summary.CompletedRefsGiven / summary.TotalRefsGiven * 100
                    : 0;
                summary.SuccessRateReceived = summary.TotalRefsReceived > 0
                    ? (double)summary.CompletedRefsReceived / summary.TotalRefsReceived * 100
                    : 0;

                summary.LastRefGivenDate = refsGiven.FirstOrDefault()?.CreatedDate;
                summary.LastRefReceivedDate = refsReceived.FirstOrDefault()?.CreatedDate;
                summary.DaysSinceLastRef = summary.LastRefGivenDate.HasValue
                    ? (DateTime.Now - summary.LastRefGivenDate.Value).Days
                    : 0;

                // Map refs given
                var refsGivenDTO = new List<UserRefGivenDTO>();
                foreach (var refItem in refsGiven)
                {
                    // Get receiver info
                    var receiver = await membershipRepo.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == refItem.RefTo);

                    refsGivenDTO.Add(new UserRefGivenDTO
                    {
                        Id = refItem.Id,
                        Content = refItem.Content ?? "",
                        RefToUserZaloId = refItem.RefTo ?? "",
                        RefToName = receiver?.Fullname ?? "Unknown",
                        RefToPhone = receiver?.PhoneNumber ?? "",
                        Value = (decimal)refItem.Value,
                        Status = GetRefStatusString(refItem.Status),
                        StatusText = GetRefStatusText(refItem.Status),
                        CreatedDate = refItem.CreatedDate,
                        CompletedDate = refItem.Status == 3 ? refItem.UpdatedDate : null,
                        Notes = "" // Ref entity doesn't have Notes property
                    });
                }

                // Map refs received
                var refsReceivedDTO = new List<UserRefReceivedDTO>();
                foreach (var refItem in refsReceived)
                {
                    // Get giver info
                    var giver = await membershipRepo.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == refItem.RefFrom);

                    refsReceivedDTO.Add(new UserRefReceivedDTO
                    {
                        Id = refItem.Id,
                        Content = refItem.Content ?? "",
                        RefFromUserZaloId = refItem.RefFrom ?? "",
                        RefFromName = giver?.Fullname ?? "Unknown",
                        RefFromPhone = giver?.PhoneNumber ?? "",
                        Value = (decimal)refItem.Value,
                        Status = GetRefStatusString(refItem.Status),
                        StatusText = GetRefStatusText(refItem.Status),
                        CreatedDate = refItem.CreatedDate,
                        CompletedDate = refItem.Status == 3 ? refItem.UpdatedDate : null,
                        Notes = "" // Ref entity doesn't have Notes property
                    });
                }

                // Get monthly data for last 12 months
                var monthlyData = await GetUserMonthlyRefDataAsync(userZaloId);

                return new UserRefDashboardDTO
                {
                    UserZaloId = membership.UserZaloId,
                    Fullname = membership.Fullname,
                    PhoneNumber = membership.PhoneNumber,
                    Avatar = membership.ZaloAvatar,
                    Company = null,
                    Position = null,
                    CreatedDate = membership.CreatedDate,
                    Summary = summary,
                    RefsGiven = refsGivenDTO,
                    RefsReceived = refsReceivedDTO,
                    MonthlyData = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ref dashboard for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<List<UserRefListDTO>> GetTopReferrersAsync(int limit = 10)
        {
            try
            {
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();

                // Get all refs given
                var refsGiven = await refRepo.AsQueryable()
                    .Where(r => !string.IsNullOrEmpty(r.RefFrom))
                    .GroupBy(r => r.RefFrom)
                    .Select(g => new
                    {
                        UserZaloId = g.Key,
                        TotalRefs = g.Count(),
                        CompletedRefs = g.Count(r => r.Status == 3),
                        TotalValue = g.Sum(r => r.Value)
                    })
                    .OrderByDescending(x => x.TotalRefs)
                    .Take(limit)
                    .ToListAsync();

                var result = new List<UserRefListDTO>();
                foreach (var refData in refsGiven)
                {
                    var membership = await membershipRepo.AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == refData.UserZaloId);

                    if (membership != null)
                    {
                        var successRate = refData.TotalRefs > 0
                            ? (double)refData.CompletedRefs / refData.TotalRefs * 100
                            : 0;

                        result.Add(new UserRefListDTO
                        {
                            UserZaloId = membership.UserZaloId,
                            Fullname = membership.Fullname,
                            PhoneNumber = membership.PhoneNumber,
                            Avatar = membership.ZaloAvatar,
                            Company = null,
                            CreatedDate = membership.CreatedDate,
                            TotalRefsGiven = refData.TotalRefs,
                            TotalRefsReceived = 0, // Not calculated for top referrers
                            TotalValueGiven = (decimal)refData.TotalValue,
                            TotalValueReceived = 0,
                            SuccessRate = successRate,
                            LastRefDate = null // Not calculated for top referrers
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top referrers");
                throw;
            }
        }

        public async Task<List<UserMonthlyRefDataDTO>> GetRefTimelineAsync(string period = "month")
        {
            try
            {
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var now = DateTime.Now;
                var startDate = period.ToLower() switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-12),
                    "year" => now.AddYears(-5),
                    _ => now.AddMonths(-12)
                };

                var refs = await refRepo.AsQueryable()
                    .Where(r => r.CreatedDate >= startDate)
                    .ToListAsync();

                var monthlyData = refs
                    .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                    .Select(g => new UserMonthlyRefDataDTO
                    {
                        Year = g.Key.Year,
                        MonthNumber = g.Key.Month,
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        RefsGiven = g.Count(r => !string.IsNullOrEmpty(r.RefFrom)),
                        CompletedRefsGiven = g.Count(r => !string.IsNullOrEmpty(r.RefFrom) && r.Status == 3),
                        ValueGiven = (decimal)g.Where(r => !string.IsNullOrEmpty(r.RefFrom)).Sum(r => r.Value),
                        RefsReceived = g.Count(r => !string.IsNullOrEmpty(r.RefTo)),
                        CompletedRefsReceived = g.Count(r => !string.IsNullOrEmpty(r.RefTo) && r.Status == 3),
                        ValueReceived = (decimal)g.Where(r => !string.IsNullOrEmpty(r.RefTo)).Sum(r => r.Value),
                        TotalRefs = g.Count(),
                        TotalValue = (decimal)g.Sum(r => r.Value),
                        SuccessRate = g.Count() > 0 ? (double)g.Count(r => r.Status == 3) / g.Count() * 100 : 0
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.MonthNumber)
                    .ToList();

                return monthlyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ref timeline");
                throw;
            }
        }

        public async Task<List<UserRefListDTO>> GetRefLeaderboardAsync(string period = "month", int limit = 20)
        {
            try
            {
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var now = DateTime.Now;
                var startDate = period.ToLower() switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    "year" => now.AddYears(-1),
                    _ => now.AddMonths(-1)
                };

                // Get refs in the specified period
                var refsInPeriod = await refRepo.AsQueryable()
                    .Where(r => r.CreatedDate >= startDate)
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
                        TotalValue = g.Sum(r => r.Value)
                    })
                    .OrderByDescending(x => x.TotalRefs)
                    .Take(limit)
                    .ToList();

                var result = new List<UserRefListDTO>();
                var ranking = 1;

                foreach (var userStat in userStats)
                {
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
                            LastRefDate = null,
                            RefRanking = ranking,
                            ValueRanking = ranking // Simplified for now
                        });

                        ranking++;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ref leaderboard");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<List<UserMonthlyRefDataDTO>> GetUserMonthlyRefDataAsync(string userZaloId)
        {
            var refRepo = _unitOfWork.GetRepository<Ref>();
            var now = DateTime.Now;
            var startDate = now.AddMonths(-24); // Get more data for better chart

            var refs = await refRepo.AsQueryable()
                .Where(r => (r.RefFrom == userZaloId || r.RefTo == userZaloId) && r.CreatedDate >= startDate)
                .ToListAsync();

            var monthlyData = refs
                .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                .Select(g => new UserMonthlyRefDataDTO
                {
                    Year = g.Key.Year,
                    MonthNumber = g.Key.Month,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    RefsGiven = g.Count(r => r.RefFrom == userZaloId),
                    CompletedRefsGiven = g.Count(r => r.RefFrom == userZaloId && r.Status == 3),
                    ValueGiven = (decimal)g.Where(r => r.RefFrom == userZaloId).Sum(r => r.Value),
                    RefsReceived = g.Count(r => r.RefTo == userZaloId),
                    CompletedRefsReceived = g.Count(r => r.RefTo == userZaloId && r.Status == 3),
                    ValueReceived = (decimal)g.Where(r => r.RefTo == userZaloId).Sum(r => r.Value),
                    TotalRefs = g.Count(),
                    TotalValue = (decimal)g.Sum(r => r.Value),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(r => r.Status == 3) / g.Count() * 100 : 0
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.MonthNumber)
                .ToList();

            // Fill missing months with zero data
            var filledData = new List<UserMonthlyRefDataDTO>();
            var currentDate = startDate;
            var endDate = now;

            while (currentDate <= endDate)
            {
                var existingData = monthlyData.FirstOrDefault(d => d.Year == currentDate.Year && d.MonthNumber == currentDate.Month);
                if (existingData != null)
                {
                    filledData.Add(existingData);
                }
                else
                {
                    filledData.Add(new UserMonthlyRefDataDTO
                    {
                        Year = currentDate.Year,
                        MonthNumber = currentDate.Month,
                        Month = currentDate.ToString("MMM yyyy"),
                        RefsGiven = 0,
                        CompletedRefsGiven = 0,
                        ValueGiven = 0,
                        RefsReceived = 0,
                        CompletedRefsReceived = 0,
                        ValueReceived = 0,
                        TotalRefs = 0,
                        TotalValue = 0,
                        SuccessRate = 0
                    });
                }
                currentDate = currentDate.AddMonths(1);
            }

            return filledData;
        }

        private string GetRefStatusText(byte status)
        {
            return status switch
            {
                1 => "Đã gửi",      // Sent
                2 => "Đã nhận",     // Received
                3 => "Hoàn thành",  // Completed
                _ => "Không xác định"
            };
        }

        private string GetRefStatusString(byte status)
        {
            return status switch
            {
                1 => "Sent",
                2 => "Received",
                3 => "Completed",
                _ => "Unknown"
            };
        }

        #endregion

        #region New Dashboard Chart APIs

        public async Task<List<DailyStatsDTO>> GetRefsDailyStatsAsync(int days = 30)
        {
            try
            {
                var refRepo = _unitOfWork.GetRepository<Ref>();
                var startDate = DateTime.Now.Date.AddDays(-days);

                var refsData = await refRepo.AsQueryable()
                    .Where(r => r.CreatedDate >= startDate)
                    .GroupBy(r => r.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var result = new List<DailyStatsDTO>();
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Now.Date.AddDays(-i);
                    var data = refsData.FirstOrDefault(r => r.Date == date);
                    result.Add(new DailyStatsDTO
                    {
                        Date = date.ToString("dd/MM"),
                        Count = data?.Count ?? 0
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs daily stats");
                throw;
            }
        }

        public async Task<RegistrationsByStatusDTO> GetRegistrationsByStatusAsync()
        {
            try
            {
                var registrationRepo = _unitOfWork.GetRepository<EventRegistration>();
                var confirmed = await registrationRepo.CountAsync(r => r.Status == 2);
                var pending = await registrationRepo.CountAsync(r => r.Status == 1);
                var cancelled = await registrationRepo.CountAsync(r => r.Status == 3);

                return new RegistrationsByStatusDTO
                {
                    Confirmed = confirmed,
                    Pending = pending,
                    Cancelled = cancelled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations by status");
                throw;
            }
        }

        public async Task<List<SubscriptionPlanStatsDTO>> GetSubscriptionPlansStatsAsync()
        {
            try
            {
                var planRepo = _unitOfWork.GetRepository<Entities.Subscriptions.SubscriptionPlan>();
                var subscriptionRepo = _unitOfWork.GetRepository<Entities.Subscriptions.MemberSubscription>();
                var now = DateTime.Now;

                var plans = await planRepo.AsQueryable().Where(p => p.IsActive).ToListAsync();
                var result = new List<SubscriptionPlanStatsDTO>();

                foreach (var plan in plans)
                {
                    var activeCount = await subscriptionRepo.CountAsync(s => s.SubscriptionPlanId == plan.Id && s.IsActive && s.EndDate > now);
                    var totalCount = await subscriptionRepo.CountAsync(s => s.SubscriptionPlanId == plan.Id);

                    result.Add(new SubscriptionPlanStatsDTO
                    {
                        PlanName = plan.PlanName,
                        ActiveCount = activeCount,
                        TotalCount = totalCount
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription plans stats");
                throw;
            }
        }

        public async Task<List<DailyStatsDTO>> GetNewMembersDailyStatsAsync(int days = 7)
        {
            try
            {
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var startDate = DateTime.Now.Date.AddDays(-days);

                var membersData = await membershipRepo.AsQueryable()
                    .Where(m => m.CreatedDate >= startDate)
                    .GroupBy(m => m.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var result = new List<DailyStatsDTO>();
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Now.Date.AddDays(-i);
                    var data = membersData.FirstOrDefault(m => m.Date == date);
                    result.Add(new DailyStatsDTO
                    {
                        Date = date.ToString("dd/MM"),
                        Count = data?.Count ?? 0
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new members daily stats");
                throw;
            }
        }

        #endregion
    }
}
