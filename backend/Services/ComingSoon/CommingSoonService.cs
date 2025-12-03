using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.ComingSoon;
using MiniAppGIBA.Models.Queries.Articles;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Queries.Meetings;
using MiniAppGIBA.Models.Queries.Showcase;
using MiniAppGIBA.Models.Response.CommingSoon;
using MiniAppGIBA.Services.Articles;
using MiniAppGIBA.Services.Events;
using MiniAppGIBA.Services.Meetings;
using MiniAppGIBA.Services.ShowCase;
using MiniAppGIBA.Services.HomePins;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Services.ComingSoon
{
    public class CommingSoonService : ICommingSoonService
    {
        private readonly IEventService _eventService;
        private readonly IArticleService _articleService;
        private readonly IMeetingService _meetingService;
        private readonly IShowcaseService _showcaseService;
        private readonly IHomePinService _homePinService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUrl _url;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<EventRegistration> _eventRegistrationRepository;
        private readonly IRepository<EventGuest> _eventGuestRepository;
        private readonly IRepository<GuestList> _guestListRepository;
        private readonly IRepository<MiniAppGIBA.Entities.Memberships.Membership> _membershipRepository;
        public CommingSoonService(
            IEventService eventService,
            IArticleService articleService,
            IMeetingService meetingService,
            IShowcaseService showcaseService,
            IHomePinService homePinService,
            IUnitOfWork unitOfWork,
            IUrl url,
            IHttpContextAccessor httpContextAccessor,
            IRepository<EventRegistration> eventRegistrationRepository)
        {
            _eventService = eventService;
            _articleService = articleService;
            _meetingService = meetingService;
            _showcaseService = showcaseService;
            _homePinService = homePinService;
            _unitOfWork = unitOfWork;
            _url = url;
            _httpContextAccessor = httpContextAccessor;
            _eventRegistrationRepository = eventRegistrationRepository;
            _eventGuestRepository = unitOfWork.GetRepository<EventGuest>();
            _guestListRepository = unitOfWork.GetRepository<GuestList>();
            _membershipRepository = unitOfWork.GetRepository<MiniAppGIBA.Entities.Memberships.Membership>();
        }

        public async Task<CommingSoonResponse> GetComingSoon(string? userZaloId = null)
        {
            var now = DateTime.Now;
            var twoDaysBefore = now.AddDays(-2);
            var sevenDaysLater = now.AddDays(7);

            var response = new CommingSoonResponse();

            // 0. Lấy HomePins trước (ưu tiên cao nhất)
            var homePinsResult = await _homePinService.GetHomePinsForUserAsync(userZaloId);
            var pinnedEventIds = new HashSet<string>();
            var pinnedMeetingIds = new HashSet<string>();
            var pinnedShowcaseIds = new HashSet<string>();

            if (homePinsResult.IsSuccess && homePinsResult.Data != null)
            {
                foreach (var pin in homePinsResult.Data.Pins)
                {
                    switch (pin.EntityType)
                    {
                        case PinEntityType.Event:
                            pinnedEventIds.Add(pin.EntityId);
                            break;
                        case PinEntityType.Meeting:
                            pinnedMeetingIds.Add(pin.EntityId);
                            break;
                        case PinEntityType.Showcase:
                            pinnedShowcaseIds.Add(pin.EntityId);
                            break;
                    }
                }
            }

            // 1. Lấy Events từ 2 ngày trước đến 7 ngày sau
            var eventQuery = new EventQueryParameters
            {
                Page = 1,
                PageSize = 20,
                StartDate = twoDaysBefore,
                EndDate = sevenDaysLater,
                IsActive = true,
                IsUser = true
            };
	            var eventsResult = await _eventService.GetEventsAsync(eventQuery, null, null, userZaloId);
	            var httpContext = _httpContextAccessor.HttpContext;

	            var eventsList = new List<ComingSoonEventDTO>();
	            foreach (var e in eventsResult.Items.Where(e => e.StartTime >= twoDaysBefore && e.StartTime <= sevenDaysLater))
	            {
	                // Convert Banner to full URL
	                var bannerFullUrl = await _url.ToFullUrl(e.Banner ?? string.Empty, httpContext);

	                // Convert Images list to full URLs
	                var imagesFullUrls = new List<string>();
	                if (e.Images != null && e.Images.Any())
	                {
	                    foreach (var image in e.Images)
	                    {
	                        if (!string.IsNullOrEmpty(image))
	                        {
	                            var fullUrl = await _url.ToFullUrl(image, httpContext);
	                            imagesFullUrls.Add(fullUrl);
	                        }
	                    }
	                }

	                // Lấy thêm chi tiết sự kiện (quà tặng, nhà tài trợ)
	                var eventDetail = await _eventService.GetEventByIdAsync(e.Id);
	                var eventGifts = eventDetail?.Gifts ?? new List<EventGiftDTO>();
	                var eventSponsors = eventDetail?.Sponsors ?? new List<EventSponsorDTO>();

	                // Đổi toàn bộ ảnh của quà tặng sang full URL
	                if (httpContext != null && eventGifts.Any())
	                {
	                    foreach (var gift in eventGifts)
	                    {
	                        if (gift.Images != null && gift.Images.Any())
	                        {
	                            var fullGiftImages = new List<string>();
	                            foreach (var img in gift.Images)
	                            {
	                                if (!string.IsNullOrEmpty(img))
	                                {
	                                    var fullUrl = await _url.ToFullUrl(img, httpContext);
	                                    fullGiftImages.Add(fullUrl);
	                                }
	                            }
	                            gift.Images = fullGiftImages;
	                        }
	                    }
	                }

	                eventsList.Add(new ComingSoonEventDTO
	                {
	                    Id = e.Id,
	                    Title = e.Title,
	                    StartTime = e.StartTime,
	                    EndTime = e.EndTime,
	                    GroupName = e.GroupName,
	                    GroupId = e.GroupId,
	                    Address = e.Address,
	                    MeetingLink = e.MeetingLink,
	                    GoogleMapURL = e.GoogleMapURL,
	                    Banner = bannerFullUrl,
	                    Images = imagesFullUrls,
	                    JoinCount = e.JoinCount,
	                    Type = e.Type,
	                    TypeText = e.TypeText,
	                    Status = e.Status,
	                    StatusText = e.StatusText,
	                    StatusClass = e.StatusClass,
	                    IsActive = e.IsActive,
	                    CreatedDate = e.CreatedDate,
	                    UpdatedDate = e.UpdatedDate,
	                    Content = e.Content,
	                    IsRegister = e.IsRegister,
	                    IsCheckIn = e.IsCheckIn,
	                    CheckInCode = e.CheckInCode,
	                    NeedApproval = e.NeedApproval,
	                    EventGifts = eventGifts,
	                    EventSponsors = eventSponsors
	                });
	            }

            // Kiểm tra EventRegistration và GuestList để cập nhật IsRegister và IsCheckIn
            if (!string.IsNullOrEmpty(userZaloId) && eventsList.Any())
            {
                // Lấy EventRegistration (đăng ký đơn lẻ)
                var eventIds = eventsList.Select(e => e.Id).ToList();
                var registrations = await _eventRegistrationRepository.AsQueryable()
                    .Where(r => r.UserZaloId == userZaloId && eventIds.Contains(r.EventId))
                    .ToListAsync();
                Console.WriteLine("sdkflksdlfk"+registrations);

                var regDict = registrations.ToDictionary(r => r.EventId, r => r);


                // Lấy GuestList (được mời trong đơn nhóm)
                // Tìm user qua Membership để lấy phone/email
                var userMembership = await _membershipRepository.AsQueryable()
                    .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

                var guestListDict = new Dictionary<string, GuestList>();
                if (userMembership != null)
                {
                    // Tìm GuestList mà user được mời (match qua phone hoặc email)
                    var guestLists = await _guestListRepository.AsQueryable()
                        .Include(gl => gl.EventGuest)
                        .Where(gl => 
                            eventIds.Contains(gl.EventGuest.EventId) &&
                            (!string.IsNullOrEmpty(userMembership.PhoneNumber) && gl.GuestPhone == userMembership.PhoneNumber)
                        )
                        .ToListAsync();

                    foreach (var guestList in guestLists)
                    {
                        if (guestList.EventGuest != null && !guestListDict.ContainsKey(guestList.EventGuest.EventId))
                        {
                            guestListDict[guestList.EventGuest.EventId] = guestList;
                        }
                    }
                }

                // Cập nhật IsRegister và IsCheckIn cho từng event
                foreach (var item in eventsList)
                {
                    // Kiểm tra EventRegistration trước
                    if (regDict.TryGetValue(item.Id, out var reg))
                    {
                        // Status 3 = Cancelled -> không được coi là đã đăng ký
                        // Các status khác (0=Pending, 1=Registered, 2=CheckedIn) -> đã đăng ký
                        item.IsRegister = reg.Status != 3;
                        item.IsCheckIn = reg.CheckInStatus == ECheckInStatus.CheckedIn;
                        item.CheckInCode = reg.Status != 3 ? reg.CheckInCode : null;
                    }
                    // Nếu chưa đăng ký đơn lẻ, kiểm tra GuestList (được mời)
                    else if (guestListDict.TryGetValue(item.Id, out var guestList))
                    {
                        // Status GuestList: 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Cancelled, 4 = PendingRegistration, 5 = Registered
                        // IsRegister = false nếu status = 2 (Rejected) hoặc 3 (Cancelled)
                        item.IsRegister = guestList.Status != 2 && guestList.Status != 3;
                        item.IsCheckIn = guestList.CheckInStatus == true;
                        item.CheckInCode = (guestList.Status != 2 && guestList.Status != 3 && !string.IsNullOrEmpty(guestList.CheckInCode)) 
                            ? $"GUEST_{guestList.CheckInCode}" 
                            : null;
                    }
                }
            }

            // 1b. Lấy pinned events và thêm vào đầu danh sách
            var finalEventsList = new List<ComingSoonEventDTO>();
            
            // Thêm pinned events trước (theo DisplayOrder)
            if (homePinsResult.IsSuccess && homePinsResult.Data != null)
            {
                var pinnedEvents = homePinsResult.Data.Pins
                    .Where(p => p.EntityType == PinEntityType.Event)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                foreach (var pin in pinnedEvents)
                {
                    var pinnedEvent = eventsList.FirstOrDefault(e => e.Id == pin.EntityId);
                    if (pinnedEvent != null)
                    {
                        pinnedEvent.IsPinned = true;
                        finalEventsList.Add(pinnedEvent);
                    }
                    else
                    {
                        // Nếu event không có trong time range, lấy riêng
                        var eventDetail = await _eventService.GetEventByIdAsync(pin.EntityId);
                        if (eventDetail != null)
                        {
                            var bannerFullUrl = await _url.ToFullUrl(eventDetail.Banner ?? string.Empty, httpContext);
                            var imagesFullUrls = new List<string>();
                            if (eventDetail.Images != null && eventDetail.Images.Any())
                            {
                                foreach (var image in eventDetail.Images)
                                {
                                    if (!string.IsNullOrEmpty(image))
                                    {
                                        var fullUrl = await _url.ToFullUrl(image, httpContext);
                                        imagesFullUrls.Add(fullUrl);
                                    }
                                }
                            }

                            // Đổi ảnh quà tặng sang full URL
                            if (httpContext != null && eventDetail.Gifts != null && eventDetail.Gifts.Any())
                            {
                                foreach (var gift in eventDetail.Gifts)
                                {
                                    if (gift.Images != null && gift.Images.Any())
                                    {
                                        var fullGiftImages = new List<string>();
                                        foreach (var img in gift.Images)
                                        {
                                            if (!string.IsNullOrEmpty(img))
                                            {
                                                var fullUrl = await _url.ToFullUrl(img, httpContext);
                                                fullGiftImages.Add(fullUrl);
                                            }
                                        }
                                        gift.Images = fullGiftImages;
                                    }
                                }
                            }

                            var pinnedEventDto = new ComingSoonEventDTO
                            {
                                Id = eventDetail.Id,
                                Title = eventDetail.Title,
                                StartTime = eventDetail.StartTime,
                                EndTime = eventDetail.EndTime,
                                GroupName = eventDetail.GroupName,
                                GroupId = eventDetail.GroupId,
                                Address = eventDetail.Address,
                                MeetingLink = eventDetail.MeetingLink,
                                GoogleMapURL = eventDetail.GoogleMapURL,
                                Banner = bannerFullUrl,
                                Images = imagesFullUrls,
                                JoinCount = eventDetail.JoinCount,
                                Type = eventDetail.Type,
                                TypeText = eventDetail.TypeText,
                                Status = eventDetail.Status,
                                StatusText = eventDetail.StatusText,
                                StatusClass = eventDetail.StatusClass,
                                IsActive = eventDetail.IsActive,
                                CreatedDate = eventDetail.CreatedDate,
                                UpdatedDate = eventDetail.UpdatedDate,
                                Content = eventDetail.Content,
                                IsRegister = eventDetail.IsRegister,
                                IsCheckIn = eventDetail.IsCheckIn,
                                CheckInCode = eventDetail.CheckInCode,
                                NeedApproval = eventDetail.NeedApproval,
                                EventGifts = eventDetail.Gifts ?? new List<EventGiftDTO>(),
                                EventSponsors = eventDetail.Sponsors ?? new List<EventSponsorDTO>()
                            };

                            pinnedEventDto.IsPinned = true;

                            // Kiểm tra IsRegister và IsCheckIn cho pinned event
                            if (!string.IsNullOrEmpty(userZaloId))
                            {
                                var reg = await _eventRegistrationRepository.AsQueryable()
                                    .FirstOrDefaultAsync(r => r.UserZaloId == userZaloId && r.EventId == pin.EntityId);

                                if (reg != null)
                                {
                                    // Status 3 = Cancelled -> không được coi là đã đăng ký
                                    pinnedEventDto.IsRegister = reg.Status != 3;
                                    pinnedEventDto.IsCheckIn = reg.CheckInStatus == ECheckInStatus.CheckedIn;
                                    pinnedEventDto.CheckInCode = reg.Status != 3 ? reg.CheckInCode : null;
                                }
                                else
                                {
                                    var userMembership = await _membershipRepository.AsQueryable()
                                        .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

                                    if (userMembership != null)
                                    {
                                        var guestList = await _guestListRepository.AsQueryable()
                                            .Include(gl => gl.EventGuest)
                                            .FirstOrDefaultAsync(gl =>
                                                gl.EventGuest.EventId == pin.EntityId &&
                                                (!string.IsNullOrEmpty(userMembership.PhoneNumber) && gl.GuestPhone == userMembership.PhoneNumber)
                                            );

                                        if (guestList != null)
                                        {
                                            // Status 2 = Rejected, 3 = Cancelled -> không được coi là đã đăng ký
                                            pinnedEventDto.IsRegister = guestList.Status != 2 && guestList.Status != 3;
                                            pinnedEventDto.IsCheckIn = guestList.CheckInStatus == true;
                                            pinnedEventDto.CheckInCode = (guestList.Status != 2 && guestList.Status != 3 && !string.IsNullOrEmpty(guestList.CheckInCode)) 
                                                ? $"GUEST_{guestList.CheckInCode}" 
                                                : null;
                                        }
                                    }
                                }
                            }

                            finalEventsList.Add(pinnedEventDto);
                        }
                    }
                }
            }

            // Thêm coming soon events (loại bỏ những cái đã pinned) và sắp xếp theo startTime
            // Nếu cùng startTime thì sắp xếp theo CreatedDate (tạo trước lên trước)
            var comingSoonEvents = eventsList
                .Where(e => !pinnedEventIds.Contains(e.Id))
                .OrderBy(e => e.CreatedDate)
                // .ThenBy(e => e.CreatedDate)
                .ToList();
            finalEventsList.AddRange(comingSoonEvents);

            response.Events = finalEventsList;

            // 2. Lấy Newsletter (Article) - CHỈ lấy bài viết được ghim (pinned)
            var pinnedArticles = (homePinsResult.IsSuccess && homePinsResult.Data != null)
                ? homePinsResult.Data.Pins
                    .Where(p => p.EntityType == PinEntityType.Article)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList()
                : new List<Models.HomePins.HomePinDto>();

            var newslettersList = new List<NewsletterDTO>();
            
            // Lấy TẤT CẢ bài viết được ghim
            foreach (var pinnedArticle in pinnedArticles)
            {
                var articleDetail = await _articleService.GetByIdAsync(pinnedArticle.EntityId);
                
                if (articleDetail != null)
                {
                    var description = string.Empty;
                    if (!string.IsNullOrEmpty(articleDetail.Content))
                    {
                        var plainText = System.Text.RegularExpressions.Regex.Replace(articleDetail.Content, "<.*?>", string.Empty);
                        description = plainText.Length > 200
                            ? plainText.Substring(0, 200) + "..."
                            : plainText;
                    }
                    var bannerImage = "uploads/images/articles/" + articleDetail.BannerImage ?? string.Empty;
                    var newsletterDto = new NewsletterDTO
                    {
                        Id = articleDetail.Id,
                        Title = articleDetail.Title,
                        Description = description ?? string.Empty,
                        BannerImage = await _url.ToFullUrl(bannerImage, httpContext),
                        Type = articleDetail.Status, // 0 = Nội bộ, 1 = Công khai
                        TypeText = articleDetail.Status == 1 ? "Công khai" : "Nội bộ"
                    };
                    newslettersList.Add(newsletterDto);
                }
            }
            
            // Set danh sách tất cả bài viết được ghim
            response.Newsletters = newslettersList;
            
            // Backward compatible: Newsletter = bài viết đầu tiên (nếu có)
            

            // 3. Lấy Meetings từ 2 ngày trước đến 7 ngày sau
            var meetingQuery = new MeetingQueryParams
            {
                Page = 1,
                PageSize = 50
            };
            var meetingsResult = await _meetingService.GetPage(meetingQuery, string.Empty, userZaloId ?? string.Empty);
            var meetingsList = meetingsResult.Items
                .Where(m => m.StartDate >= twoDaysBefore && m.StartDate <= sevenDaysLater)
                .Select(m => new MeetingDTO
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.StartDate,
                    MeetingType = (int)m.MeetingType,
                    Location = m.Location,
                    MeetingLink = m.MeetingLink,
                    GroupName = m.GroupName,
                    Type = m.IsPublic ? 2 : 1, // 1 = Nội bộ, 2 = Công khai
                    TypeText = m.IsPublic ? "Công khai" : "Nội bộ"
                })
                .ToList();

            // 3b. Merge pinned meetings
            var finalMeetingsList = new List<MeetingDTO>();
            
            // Thêm pinned meetings trước
            if (homePinsResult.IsSuccess && homePinsResult.Data != null)
            {
                var pinnedMeetings = homePinsResult.Data.Pins
                    .Where(p => p.EntityType == PinEntityType.Meeting)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                foreach (var pin in pinnedMeetings)
                {
                    var pinnedMeeting = meetingsList.FirstOrDefault(m => m.Id == pin.EntityId);
                    if (pinnedMeeting != null)
                    {
                        pinnedMeeting.IsPinned = true;
                        finalMeetingsList.Add(pinnedMeeting);
                    }
                    else
                    {
                        // Lấy meeting ngoài time range từ database
                        var meetingEntity = await _meetingService.GetByIdAsync(pin.EntityId);
                        if (meetingEntity != null)
                        {
                            finalMeetingsList.Add(new MeetingDTO
                            {
                                Id = meetingEntity.Id,
                                Title = meetingEntity.Title,
                                Time = meetingEntity.StartDate,
                                MeetingType = (int)meetingEntity.MeetingType,
                                Location = meetingEntity.Location,
                                MeetingLink = meetingEntity.MeetingLink,
                                GroupName = meetingEntity.GroupName,
                                IsPinned = true,
                                Type = meetingEntity.IsPublic ? 2 : 1,
                                TypeText = meetingEntity.IsPublic ? "Công khai" : "Nội bộ"
                            });
                        }
                    }
                }
            }

            // Thêm coming soon meetings và sắp xếp theo time
            var comingSoonMeetings = meetingsList
                .Where(m => !pinnedMeetingIds.Contains(m.Id))
                .OrderBy(m => m.Time)
                .ToList();
            finalMeetingsList.AddRange(comingSoonMeetings);
            response.Meetings = finalMeetingsList;

            // 4. Lấy Showcases từ 2 ngày trước đến 7 ngày sau
            var showcaseQuery = new ShowcaseQueryParams
            {
                Page = 1,
                PageSize = 50
            };
            var showcasesResult = await _showcaseService.GetPage(showcaseQuery, string.Empty, userZaloId ?? string.Empty);
            var showcasesList = showcasesResult.Items
                .Where(s => s.StartDate >= twoDaysBefore && s.StartDate <= sevenDaysLater)
                .Select(s => new ShowcaseDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    MembershipName = s.MembershipName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Location = s.Location,
                    GroupName = s.GroupName,
                    Type = s.IsPublic ? 2 : 1, // 1 = Nội bộ, 2 = Công khai
                    TypeText = s.IsPublic ? "Công khai" : "Nội bộ"
                })
                .ToList();

            // 4b. Merge pinned showcases
            var finalShowcasesList = new List<ShowcaseDTO>();
            
            // Thêm pinned showcases trước
            if (homePinsResult.IsSuccess && homePinsResult.Data != null)
            {
                var pinnedShowcases = homePinsResult.Data.Pins
                    .Where(p => p.EntityType == PinEntityType.Showcase)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                foreach (var pin in pinnedShowcases)
                {
                    var pinnedShowcase = showcasesList.FirstOrDefault(s => s.Id == pin.EntityId);
                    if (pinnedShowcase != null)
                    {
                        pinnedShowcase.IsPinned = true;
                        finalShowcasesList.Add(pinnedShowcase);
                    }
                    else
                    {
                        // Lấy showcase ngoài time range từ database
                        var showcaseEntity = await _showcaseService.GetByIdAsync(pin.EntityId);
                        if (showcaseEntity != null)
                        {
                            finalShowcasesList.Add(new ShowcaseDTO
                            {
                                Id = showcaseEntity.Id,
                                Title = showcaseEntity.Title,
                                MembershipName = showcaseEntity.MembershipName,
                                StartDate = showcaseEntity.StartDate,
                                EndDate = showcaseEntity.EndDate,
                                Location = showcaseEntity.Location,
                                GroupName = showcaseEntity.GroupName,
                                IsPinned = true,
                                Type = showcaseEntity.IsPublic ? 2 : 1,
                                TypeText = showcaseEntity.IsPublic ? "Công khai" : "Nội bộ"
                            });
                        }
                    }
                }
            }

            // Thêm coming soon showcases và sắp xếp theo startDate
            var comingSoonShowcases = showcasesList
                .Where(s => !pinnedShowcaseIds.Contains(s.Id))
                .OrderBy(s => s.StartDate)
                .ToList();
            finalShowcasesList.AddRange(comingSoonShowcases);
            response.Showcases = finalShowcasesList;

            return response;
        }
    }
}