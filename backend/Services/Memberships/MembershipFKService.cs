// using MiniAppGIBA.Base.Interface;
// using MiniAppGIBA.Entities.Memberships;
// using MiniAppGIBA.Entities.Groups;
// using MiniAppGIBA.Entities.Events;
// using MiniAppGIBA.Models.DTOs.Memberships;
// using Microsoft.EntityFrameworkCore;

// namespace MiniAppGIBA.Services.Memberships
// {
//     /// <summary>
//     /// Service sử dụng Foreign Keys thay vì Navigation Properties
//     /// Đây là cách tiếp cận mới, hiệu quả hơn cho các query phức tạp
//     /// </summary>
//     public class MembershipFKService
//     {
//         private readonly IUnitOfWork _unitOfWork;
//         private readonly ILogger<MembershipFKService> _logger;

//         public MembershipFKService(IUnitOfWork unitOfWork, ILogger<MembershipFKService> logger)
//         {
//             _unitOfWork = unitOfWork;
//             _logger = logger;
//         }

//         /// <summary>
//         /// Lấy danh sách members với thông tin groups (sử dụng FK)
//         /// </summary>
//         public async Task<List<MembershipWithGroupsDTO>> GetMembersWithGroupsAsync()
//         {
//             try
//             {
//                 var membershipRepo = _unitOfWork.GetRepository<Membership>();
//                 var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
//                 var groupRepo = _unitOfWork.GetRepository<Group>();

//                 // Query sử dụng FK thay vì navigation properties
//                 var memberships = await membershipRepo.AsQueryable()
//                     .AsNoTracking()
//                     .ToListAsync();

//                 var membershipGroups = await membershipGroupRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(mg => mg.IsApproved == true) // Chỉ lấy approved
//                     .ToListAsync();

//                 var groupIds = membershipGroups.Select(mg => mg.GroupId).Distinct().ToList();
//                 var groups = await groupRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(g => groupIds.Contains(g.Id))
//                     .ToListAsync();

//                 // Join data manually (thay vì dùng Include)
//                 var result = memberships.Select(m => new MembershipWithGroupsDTO
//                 {
//                     Id = m.Id,
//                     UserZaloId = m.UserZaloId,
//                     Fullname = m.Fullname,
//                     PhoneNumber = m.PhoneNumber,
//                     Email = m.Email,
//                     Company = m.Company,
//                     Position = m.Position,
//                     ZaloAvatar = m.ZaloAvatar,
//                     CreatedDate = m.CreatedDate,

//                     // Groups được join bằng FK
//                     Groups = membershipGroups
//                         .Where(mg => mg.UserZaloId == m.UserZaloId)
//                         .Join(groups, mg => mg.GroupId, g => g.Id, (mg, g) => new GroupSummaryDTO
//                         {
//                             Id = g.Id,
//                             GroupName = g.GroupName,
//                             Description = g.Description,
//                             JoinedDate = mg.CreatedDate,
//                             IsActive = g.IsActive
//                         })
//                         .ToList()
//                 }).ToList();

//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting members with groups using FK");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Lấy thống kê membership theo groups (sử dụng FK)
//         /// </summary>
//         public async Task<Dictionary<string, int>> GetMembershipStatsByGroupAsync()
//         {
//             try
//             {
//                 var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();
//                 var groupRepo = _unitOfWork.GetRepository<Group>();

//                 // Query sử dụng FK
//                 var stats = await membershipGroupRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(mg => mg.IsApproved == true)
//                     .GroupBy(mg => mg.GroupId)
//                     .Select(g => new { GroupId = g.Key, Count = g.Count() })
//                     .ToListAsync();

//                 var groupIds = stats.Select(s => s.GroupId).ToList();
//                 var groups = await groupRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(g => groupIds.Contains(g.Id))
//                     .ToListAsync();

//                 // Join để lấy tên group
//                 var result = stats
//                     .Join(groups, s => s.GroupId, g => g.Id, (s, g) => new { g.GroupName, s.Count })
//                     .ToDictionary(x => x.GroupName, x => x.Count);

//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting membership stats by group using FK");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Lấy members đã tham gia events (sử dụng FK)
//         /// </summary>
//         public async Task<List<MemberEventParticipationDTO>> GetMemberEventParticipationAsync()
//         {
//             try
//             {
//                 var membershipRepo = _unitOfWork.GetRepository<Membership>();
//                 var eventRegistrationRepo = _unitOfWork.GetRepository<EventRegistration>();
//                 var eventRepo = _unitOfWork.GetRepository<Event>();

//                 // Query sử dụng FK
//                 var memberships = await membershipRepo.AsQueryable()
//                     .AsNoTracking()
//                     .ToListAsync();

//                 var registrations = await eventRegistrationRepo.AsQueryable()
//                     .AsNoTracking()
//                     .ToListAsync();

//                 var eventIds = registrations.Select(er => er.EventId).Distinct().ToList();
//                 var events = await eventRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(e => eventIds.Contains(e.Id))
//                     .ToListAsync();

//                 // Join data manually
//                 var result = memberships.Select(m => new MemberEventParticipationDTO
//                 {
//                     UserZaloId = m.UserZaloId,
//                     Fullname = m.Fullname,
//                     PhoneNumber = m.PhoneNumber,
//                     Company = m.Company,

//                     // Events được join bằng FK
//                     ParticipatedEvents = registrations
//                         .Where(er => er.UserZaloId == m.UserZaloId)
//                         .Join(events, er => er.EventId, e => e.Id, (er, e) => new EventSummaryDTO
//                         {
//                             Id = e.Id,
//                             EventName = e.EventName,
//                             StartTime = e.StartTime,
//                             EndTime = e.EndTime,
//                             RegistrationDate = er.CreatedDate,
//                             IsCheckedIn = er.IsCheckedIn
//                         })
//                         .ToList()
//                 }).Where(m => m.ParticipatedEvents.Any())
//                 .ToList();

//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting member event participation using FK");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Lấy top members có nhiều groups nhất (sử dụng FK)
//         /// </summary>
//         public async Task<List<TopMemberDTO>> GetTopMembersByGroupCountAsync(int limit = 10)
//         {
//             try
//             {
//                 var membershipRepo = _unitOfWork.GetRepository<Membership>();
//                 var membershipGroupRepo = _unitOfWork.GetRepository<MembershipGroup>();

//                 // Query sử dụng FK
//                 var topMembers = await membershipGroupRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(mg => mg.IsApproved == true)
//                     .GroupBy(mg => mg.UserZaloId)
//                     .Select(g => new { UserZaloId = g.Key, GroupCount = g.Count() })
//                     .OrderByDescending(x => x.GroupCount)
//                     .Take(limit)
//                     .ToListAsync();

//                 var userZaloIds = topMembers.Select(tm => tm.UserZaloId).ToList();
//                 var memberships = await membershipRepo.AsQueryable()
//                     .AsNoTracking()
//                     .Where(m => userZaloIds.Contains(m.UserZaloId))
//                     .ToListAsync();

//                 // Join để lấy thông tin member
//                 var result = topMembers
//                     .Join(memberships, tm => tm.UserZaloId, m => m.UserZaloId, (tm, m) => new TopMemberDTO
//                     {
//                         UserZaloId = m.UserZaloId,
//                         Fullname = m.Fullname,
//                         PhoneNumber = m.PhoneNumber,
//                         Company = m.Company,
//                         GroupCount = tm.GroupCount,
//                         CreatedDate = m.CreatedDate
//                     })
//                     .OrderByDescending(x => x.GroupCount)
//                     .ToList();

//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting top members by group count using FK");
//                 throw;
//             }
//         }
//     }

//     // DTOs for FK-based queries
//     public class MembershipWithGroupsDTO
//     {
//         public string Id { get; set; } = string.Empty;
//         public string UserZaloId { get; set; } = string.Empty;
//         public string Fullname { get; set; } = string.Empty;
//         public string PhoneNumber { get; set; } = string.Empty;
//         public string? Email { get; set; }
//         public string? Company { get; set; }
//         public string? Position { get; set; }
//         public string? ZaloAvatar { get; set; }
//         public DateTime CreatedDate { get; set; }
//         public List<GroupSummaryDTO> Groups { get; set; } = new List<GroupSummaryDTO>();
//     }

//     public class GroupSummaryDTO
//     {
//         public string Id { get; set; } = string.Empty;
//         public string GroupName { get; set; } = string.Empty;
//         public string? Description { get; set; }
//         public DateTime JoinedDate { get; set; }
//         public bool IsActive { get; set; }
//     }

//     public class MemberEventParticipationDTO
//     {
//         public string UserZaloId { get; set; } = string.Empty;
//         public string Fullname { get; set; } = string.Empty;
//         public string PhoneNumber { get; set; } = string.Empty;
//         public string? Company { get; set; }
//         public List<EventSummaryDTO> ParticipatedEvents { get; set; } = new List<EventSummaryDTO>();
//     }

//     public class EventSummaryDTO
//     {
//         public string Id { get; set; } = string.Empty;
//         public string EventName { get; set; } = string.Empty;
//         public DateTime StartTime { get; set; }
//         public DateTime EndTime { get; set; }
//         public DateTime RegistrationDate { get; set; }
//         public bool IsCheckedIn { get; set; }
//     }

//     public class TopMemberDTO
//     {
//         public string UserZaloId { get; set; } = string.Empty;
//         public string Fullname { get; set; } = string.Empty;
//         public string PhoneNumber { get; set; } = string.Empty;
//         public string? Company { get; set; }
//         public int GroupCount { get; set; }
//         public DateTime CreatedDate { get; set; }
//     }
// }
