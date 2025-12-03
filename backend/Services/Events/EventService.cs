using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Sponsors;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;
using System.Text.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Enum;


namespace MiniAppGIBA.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;      
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<EventGift> _eventGiftRepository;
        private readonly IRepository<EventSponsor> _eventSponsorRepository;
        private readonly IRepository<EventGuest> _eventGuestRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<GroupPermission> _groupPermissionRepository;
        private readonly ILogger<EventService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<EventRegistration> _eventRegistrationRepository;
        private readonly IRepository<EventCustomField> _eventCustomFieldRepository;
        private readonly IRepository<GuestList> _guestListRepository;
        public EventService(
            IUnitOfWork unitOfWork,
            IRepository<Event> eventRepository,
            IRepository<EventGift> eventGiftRepository,
            IRepository<EventSponsor> eventSponsorRepository,
            IRepository<EventGuest> eventGuestRepository,
            IRepository<Group> groupRepository,
            IRepository<GroupPermission> groupPermissionRepository,
            ILogger<EventService> logger,
            IRepository<MembershipGroup> membershipGroupRepository,
            IRepository<EventRegistration> eventRegistrationRepository,
            IRepository<GuestList> guestListRepository,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _eventRepository = eventRepository;
            _eventGiftRepository = eventGiftRepository;
            _eventSponsorRepository = eventSponsorRepository;
            _eventGuestRepository = eventGuestRepository;
            _groupRepository = groupRepository;
            _groupPermissionRepository = groupPermissionRepository;
            _logger = logger;
            _env = env;
            _membershipGroupRepository = membershipGroupRepository;
            _eventRegistrationRepository = eventRegistrationRepository;
            _eventCustomFieldRepository = unitOfWork.GetRepository<EventCustomField>();
            _guestListRepository = guestListRepository;
        }

        public async Task<PagedResult<EventDTO>> GetEventsAsync(EventQueryParameters query, string? roleName = null, string? userId = null, string? userZaloId = null)
        {
            try
            {
                IQueryable<Event> queryable = _eventRepository.AsQueryable()
                    .Include(e => e.Group);

                if (!string.IsNullOrEmpty(roleName) && !query.IsUser)
                {
                    _logger.LogDebug("Applying role-based filters for role: {RoleName}", roleName);
                    queryable = await ApplyRoleBasedFiltersAsync(queryable, roleName, userId, query, userZaloId);
                }
                else
                {
                    _logger.LogDebug("Applying user-based filters");
                    queryable = await ApplyUserBasedFiltersAsync(queryable, query, userZaloId);
                }

                queryable = ApplyQueryFilters(queryable, query);

                if (query.IsUser || string.IsNullOrEmpty(roleName))
                {
                    queryable = await ApplyUserTypeFilterAsync(queryable, query, userZaloId);
                }

                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                var items = await queryable
                    .OrderByDescending(e => e.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var eventDTOs = items.Select(MapToEventDTO).ToList();

                if (!string.IsNullOrEmpty(userZaloId))

                {
                    await EnrichEventDTOsWithRegistrationsAsync(eventDTOs, userZaloId);
                }

                return new PagedResult<EventDTO>
                {
                    Items = eventDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events with query: {@Query}", query);
                throw;
            }
        }

        #region GetEventsAsync Helper Methods

        private async Task<IQueryable<Event>> ApplyRoleBasedFiltersAsync(
            IQueryable<Event> queryable,
            string roleName,
            string? userId,
            EventQueryParameters query,
            string? userZaloId)
        {
            // GIBA has full access - no role-based filtering needed
            if (roleName == CTRole.GIBA)
            {
                // Return all events without filtering
                return queryable;
            }

            // For non-admin users, apply user-based filters
            return await ApplyUserBasedFiltersAsync(queryable, query, userZaloId);
        }

        private async Task<IQueryable<Event>> ApplyUserBasedFiltersAsync(
            IQueryable<Event> queryable,
            EventQueryParameters query,
            string? userZaloId)
        {
            // Handle "me" filter - show events user has joined
            if (query.GroupType == "me")
            {
                var joinedEventIds = await GetJoinedEventIdsAsync(userZaloId);
                queryable = queryable.Where(e => joinedEventIds.Contains(e.Id));
            }

            return queryable;
        }

        private IQueryable<Event> ApplyQueryFilters(IQueryable<Event> queryable, EventQueryParameters query)
        {
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                queryable = queryable.Where(e =>
                    e.Title.Contains(query.Keyword) ||
                    (e.Content != null && e.Content.Contains(query.Keyword)));
            }

            if (!string.IsNullOrEmpty(query.GroupId))
            {
                queryable = queryable.Where(e => e.GroupId == query.GroupId);
            }

            if (query.Type.HasValue)
            {
                queryable = queryable.Where(e => e.Type == query.Type.Value);
            }

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(e => e.Status == query.Status.Value);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(e => e.IsActive == query.IsActive.Value);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(e => e.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                // Filter theo StartTime thay vì EndTime để lấy sự kiện bắt đầu trong khoảng thời gian
                queryable = queryable.Where(e => e.StartTime <= query.EndDate.Value);
            }

            return queryable;
        }

        private async Task<IQueryable<Event>> ApplyUserTypeFilterAsync(
            IQueryable<Event> queryable,
            EventQueryParameters query,
            string? userZaloId)
        {
            // Nếu đang dùng GroupType = "me" thì đã filter theo các sự kiện user đã tham gia ở bước trước
            if (query.GroupType == "me")
            {
                return queryable;
            }

            // User chưa đăng nhập -> chỉ thấy sự kiện công khai (Type = 2)
            if (string.IsNullOrEmpty(userZaloId))
            {
                return queryable.Where(e => e.Type == 2);
            }

            // User đã đăng nhập:
            // 1. Luôn lấy sự kiện công khai (Type = 2)
            // 2. Lấy thêm sự kiện nội bộ (Type = 1) mà user tham gia

            // Lấy danh sách group mà user là member
            var userJoinedGroupIds = await _membershipGroupRepository.AsQueryable()
                .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                .Select(mg => mg.GroupId)
                .ToListAsync();

            var userJoinedGroupIdsList = userJoinedGroupIds.ToList();

            // Lấy các Event mà user đã đăng ký (EventRegistration)
            var joinedEventIds = new List<string>();
            var registeredEventIds = await _eventRegistrationRepository.AsQueryable()
                .Where(r => r.UserZaloId == userZaloId && r.Status != (byte)ERegistrationStatus.Cancelled)
                .Select(r => r.EventId)
                .Distinct()
                .ToListAsync();
            joinedEventIds.AddRange(registeredEventIds);

            // Lấy các Event mà user được mời trong GuestList
            var membershipRepository = _unitOfWork.GetRepository<Membership>();
            var userMembership = await membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

            if (userMembership != null)
            {
                var phone = userMembership.PhoneNumber;
                if (!string.IsNullOrEmpty(phone))
                {
                    var guestListRepository = _unitOfWork.GetRepository<GuestList>();
                    var guestEventIds = await guestListRepository.AsQueryable()
                        .Where(gl =>
                            gl.EventGuest != null &&
                            gl.Status != (byte)EGuestStatus.Pending &&
                            gl.Status != (byte)EGuestStatus.Rejected &&
                            gl.Status != (byte)EGuestStatus.Cancelled &&
                            !string.IsNullOrEmpty(phone) && gl.GuestPhone == phone)
                        .Select(gl => gl.EventGuest.EventId)
                        .Distinct()
                        .ToListAsync();

                    joinedEventIds.AddRange(guestEventIds);
                }
            }

            var joinedEventIdsList = joinedEventIds.Distinct().ToList();

            // Quyền xem sự kiện cho user đã đăng nhập:
            // - Sự kiện công khai (Type = 2) - LUÔN LUÔN
            // - Sự kiện nội bộ (Type = 1) của các group mà user là member
            // - Sự kiện nội bộ (Type = 1) mà user đã tham gia (EventRegistration/GuestList)
            queryable = queryable.Where(e =>
                e.Type == 2 ||
                (e.Type == 1 && (userJoinedGroupIdsList.Contains(e.GroupId) || joinedEventIdsList.Contains(e.Id))));

            return queryable;
        }

        private async Task EnrichEventDTOsWithRegistrationsAsync(List<EventDTO> eventDTOs, string userZaloId)
        {
            if (!eventDTOs.Any())
            {
                return;
            }

            var eventIds = eventDTOs.Select(e => e.Id).ToList();
            var registrations = await _eventRegistrationRepository.AsQueryable()
                .Where(r => r.UserZaloId == userZaloId && eventIds.Contains(r.EventId))
                .ToListAsync();

            var regDict = registrations.ToDictionary(r => r.EventId, r => r);

            var membershipRepository = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Memberships.Membership>();
            var guestListRepository = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Events.GuestList>();

            var userMembership = await membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

            var guestListDict = new Dictionary<string, MiniAppGIBA.Entities.Events.GuestList>();
            if (userMembership != null)
            {
                var guestLists = await guestListRepository.AsQueryable()
                    .Include(gl => gl.EventGuest)
                    .Where(gl =>
                        gl.EventGuest != null &&
                        eventIds.Contains(gl.EventGuest.EventId) &&
                        (
                            (!string.IsNullOrEmpty(userMembership.PhoneNumber) && gl.GuestPhone == userMembership.PhoneNumber) 
                            // || (!string.IsNullOrEmpty(userMembership.Email) && gl.GuestEmail == userMembership.Email)
                        ))
                    .ToListAsync();

                foreach (var guest in guestLists)
                {
                    if (guest.EventGuest != null && !guestListDict.ContainsKey(guest.EventGuest.EventId))
                    {
                        guestListDict[guest.EventGuest.EventId] = guest;
                    }
                }
            }

            foreach (var item in eventDTOs)
            {
                if (regDict.TryGetValue(item.Id, out var reg))
                {
                    item.IsRegister = reg.Status != 3 ? true : false;
                    item.IsCheckIn = reg.CheckInStatus == ECheckInStatus.CheckedIn;
                    item.CheckInCode = reg.CheckInCode;
                }
                else if (guestListDict.TryGetValue(item.Id, out var guest))
                {
                    item.IsRegister = guest.Status != 0 ;
                    item.IsCheckIn = guest.CheckInStatus == true;
                    item.CheckInCode = !string.IsNullOrEmpty(guest.CheckInCode) ? $"GUEST_{guest.CheckInCode}" : null;
                }
            }
        }

        private async Task<List<string>> GetJoinedEventIdsAsync(string? userZaloId)
        {
            if (string.IsNullOrEmpty(userZaloId))
            {
                return new List<string>();
            }

            return await _eventGuestRepository.AsQueryable()
                .Where(eg => eg.UserZaloId == userZaloId && eg.Status == (byte)EGuestStatus.Approved)
                .Select(eg => eg.EventId)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<string>> GetAllowedGroupIdsAsync(System.Linq.Expressions.Expression<Func<Group, bool>> predicate)
        {
            return await _groupRepository.AsQueryable()
                .Where(predicate)
                .Select(g => g.Id)
                .ToListAsync();
        }

        private IQueryable<Event> ApplyGroupIdFilter(IQueryable<Event> queryable, List<string>? allowedGroupIds)
        {
            if (allowedGroupIds == null || !allowedGroupIds.Any())
            {
                return queryable;
            }

            var allowedGroupIdsList = allowedGroupIds.ToList();
            return queryable.Where(e => allowedGroupIdsList.Contains(e.GroupId));
        }

        #endregion

        public async Task<EventDetailDTO?> GetEventByIdAsync(string id)
        {
            try
            {
                var eventEntity = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Include(e => e.EventGifts)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.Sponsor)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.SponsorshipTier)
                    // .Include(e => e.EventRegistrations)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventEntity == null)
                {
                    return null;
                }

                return MapToEventDetailDTO(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by id: {EventId}", id);
                throw;
            }
        }

        public async Task<EventDTO> CreateEventAsync(CreateEventRequest request)
        {
            try
            {
                var eventEntity = new Event
                {
                    GroupId = request.GroupId,
                    Title = request.Title,
                    Content = request.Content ?? string.Empty,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Type = request.Type,
                    JoinCount = request.JoinCount,
                    MeetingLink = request.MeetingLink,
                    GoogleMapURL = request.GoogleMapURL,
                    Address = request.Address,
                    IsActive = request.IsActive,
                    NeedApproval = request.NeedApproval,
                    Status = GetEventStatus(request.StartTime, request.EndTime),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Process banner upload
                if (request.BannerFile != null)
                {
                    eventEntity.Banner = await ProcessBannerUpload(request.BannerFile);
                }

                // Process single image upload
                if (request.ImageFiles != null)
                {
                    eventEntity.Images = await ProcessSingleImageUpload(request.ImageFiles);
                }

                await _eventRepository.AddAsync(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                // Process gifts if provided
                if (!string.IsNullOrEmpty(request.GiftsData))
                {
                    var giftImages = request.GiftImages != null ? new List<IFormFile> { request.GiftImages } : null;
                    await ProcessEventGifts(eventEntity.Id, request.GiftsData, giftImages);
                }

                // Process sponsor if provided
                if (!string.IsNullOrEmpty(request.SponsorId) && !string.IsNullOrEmpty(request.SponsorshipTierId))
                {
                    await ProcessEventSponsor(eventEntity.Id, request.SponsorId, request.SponsorshipTierId);
                }

                // Process custom fields if provided
                if (!string.IsNullOrEmpty(request.CustomFieldsData))
                {
                    await ProcessEventCustomFields(eventEntity.Id, request.CustomFieldsData);
                }

                // Load with navigation properties for DTO mapping
                var createdEvent = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Include(e => e.EventGifts)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.Sponsor)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.SponsorshipTier)
                    .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

                return MapToEventDTO(createdEvent!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event with request: {@Request}", request);
                throw;
            }
        }

        public async Task<EventDTO> UpdateEventAsync(string id, UpdateEventRequest request)
        {
            try
            {
                var eventEntity = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventEntity == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy sự kiện");
                }

                eventEntity.GroupId = request.GroupId;
                eventEntity.Title = request.Title;
                eventEntity.Content = request.Content ?? string.Empty;
                eventEntity.StartTime = request.StartTime;
                eventEntity.EndTime = request.EndTime;
                eventEntity.Type = request.Type;
                eventEntity.JoinCount = request.JoinCount;
                eventEntity.MeetingLink = request.MeetingLink;
                eventEntity.GoogleMapURL = request.GoogleMapURL;
                eventEntity.Address = request.Address;
                eventEntity.IsActive = request.IsActive;
                eventEntity.NeedApproval = request.NeedApproval;
                eventEntity.Status = GetEventStatus(request.StartTime, request.EndTime);
                eventEntity.UpdatedDate = DateTime.Now;

                // Process banner upload
                if (request.BannerFile != null)
                {
                    // Delete old banner if exists
                    if (!string.IsNullOrEmpty(eventEntity.Banner))
                    {
                        DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/events", eventEntity.Banner));
                    }
                    eventEntity.Banner = await ProcessBannerUpload(request.BannerFile);
                }

                // Process single image upload
                if (request.ImageFiles != null)
                {
                    // Delete old images if exists
                    if (!string.IsNullOrEmpty(eventEntity.Images))
                    {
                        var oldImages = eventEntity.Images.Split(',');
                        foreach (var oldImage in oldImages)
                        {
                            DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/events", oldImage.Trim()));
                        }
                    }
                    eventEntity.Images = await ProcessSingleImageUpload(request.ImageFiles);
                }

                _eventRepository.Update(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                // Process gifts if provided
                if (!string.IsNullOrEmpty(request.GiftsData))
                {
                    // Delete existing gifts first
                    await DeleteExistingEventGifts(eventEntity.Id);

                    // Add new gifts
                    var giftImages = request.GiftImages != null ? new List<IFormFile> { request.GiftImages } : null;
                    await ProcessEventGifts(eventEntity.Id, request.GiftsData, giftImages);
                }

                // Update sponsor if provided
                await UpdateEventSponsor(eventEntity.Id, request.SponsorId, request.SponsorshipTierId);

                // Process custom fields if provided
                if (!string.IsNullOrEmpty(request.CustomFieldsData))
                {
                    // Delete existing custom fields first
                    await DeleteExistingEventCustomFields(eventEntity.Id);
                    // Add new custom fields
                    await ProcessEventCustomFields(eventEntity.Id, request.CustomFieldsData);
                }

                // Load updated event with all navigation properties
                var updatedEvent = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Include(e => e.EventGifts)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.Sponsor)
                    .Include(e => e.EventSponsors)
                        .ThenInclude(es => es.SponsorshipTier)
                    .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

                return MapToEventDTO(updatedEvent!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteEventAsync(string id)
        {
            try
            {
                var eventEntity = await _eventRepository.AsQueryable()
                    .Include(e => e.EventRegistrations)
                    .Include(e => e.EventGifts)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventEntity == null)
                {
                    return false;
                }

                // Check if event has registrations
                if (eventEntity.EventRegistrations?.Any() == true)
                {
                    throw new InvalidOperationException("Không thể xóa sự kiện đã có người đăng ký");
                }

                // Delete associated files
                if (!string.IsNullOrEmpty(eventEntity.Banner))
                {
                    DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/events", eventEntity.Banner));
                }

                if (!string.IsNullOrEmpty(eventEntity.Images))
                {
                    var images = eventEntity.Images.Split(',');
                    foreach (var image in images)
                    {
                        DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/events", image.Trim()));
                    }
                }

                _eventRepository.Delete(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleEventStatusAsync(string id)
        {
            try
            {
                var eventEntity = await _eventRepository.FindByIdAsync(id);
                if (eventEntity == null)
                {
                    return false;
                }

                eventEntity.IsActive = !eventEntity.IsActive;
                eventEntity.UpdatedDate = DateTime.Now;

                _eventRepository.Update(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling event status {EventId}", id);
                throw;
            }
        }

        public async Task<List<EventDTO>> GetActiveEventsAsync(List<string>? allowedGroupIds = null)
        {
            try
            {
                IQueryable<Event> queryable = _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Where(e => e.IsActive);

                // Filter by allowed group IDs (for ADMIN users)
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    // Materialize list để tránh collation conflict
                    var allowedGroupIdsList = allowedGroupIds.ToList();
                    queryable = queryable.Where(e => allowedGroupIdsList.Contains(e.GroupId));
                }

                var events = await queryable
                    .OrderBy(e => e.StartTime)
                    .ToListAsync();

                return events.Select(MapToEventDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active events");
                throw;
            }
        }

        public async Task<List<EventDTO>> GetEventsByGroupAsync(string groupId)
        {
            try
            {
                var events = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Where(e => e.GroupId == groupId && e.IsActive)
                    .OrderByDescending(e => e.StartTime)
                    .ToListAsync();

                return events.Select(MapToEventDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by group {GroupId}", groupId);
                throw;
            }
        }

        #region Private Methods

        private async Task<string> ProcessBannerUpload(IFormFile bannerFile)
        {
            var savePath = Path.Combine(_env.WebRootPath, "uploads/images/events");
            var fileName = await FileHandler.SaveFile(bannerFile, savePath);
            return $"/uploads/images/events/{fileName}";
        }

        private async Task<string> ProcessSingleImageUpload(IFormFile imageFile)
        {
            var savePath = Path.Combine(_env.WebRootPath, "uploads/images/events");
            var fileName = await FileHandler.SaveFile(imageFile, savePath);
            return $"/uploads/images/events/{fileName}";
        }

        private async Task<string> ProcessImagesUpload(List<IFormFile> imageFiles)
        {
            var savePath = Path.Combine(_env.WebRootPath, "uploads/images/events");
            var fileNames = new List<string>();

            foreach (var file in imageFiles)
            {
                var fileName = await FileHandler.SaveFile(file, savePath);
                fileNames.Add(fileName);
            }

            return string.Join(",", fileNames);
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            }
        }

        private byte GetEventStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (now < startTime)
                return 1; // Sắp diễn ra
            else if (now >= startTime && now <= endTime)
                return 2; // Đang diễn ra
            else
                return 3; // Đã kết thúc
        }

        private EventDTO MapToEventDTO(Event eventEntity)
        {
            // Auto-calculate status based on current time
            var currentStatus = GetEventStatus(eventEntity.StartTime, eventEntity.EndTime);

            return new EventDTO
            {
                Id = eventEntity.Id,
                GroupId = eventEntity.GroupId,
                GroupName = eventEntity.Group?.GroupName ?? "N/A",
                Title = eventEntity.Title,
                Content = eventEntity.Content,
                Banner = eventEntity.Banner,
                Images = string.IsNullOrEmpty(eventEntity.Images)
                    ? new List<string>()
                    : eventEntity.Images.Split(',').Select(s => s.Trim()).ToList(),
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                JoinCount = eventEntity.JoinCount,
                Type = eventEntity.Type,
                TypeText = eventEntity.Type == 1 ? "Nội bộ" : "Công khai",
                MeetingLink = eventEntity.MeetingLink,
                GoogleMapURL = eventEntity.GoogleMapURL,
                Address = eventEntity.Address,
                Status = currentStatus,
                StatusText = GetStatusText(currentStatus),
                StatusClass = GetStatusClass(currentStatus),
                IsActive = eventEntity.IsActive,
                CreatedDate = eventEntity.CreatedDate,
                UpdatedDate = eventEntity.UpdatedDate,
                NeedApproval = eventEntity.NeedApproval
            };
        }

        private EventDetailDTO MapToEventDetailDTO(Event eventEntity)
        {
            // Auto-calculate status based on current time
            var currentStatus = GetEventStatus(eventEntity.StartTime, eventEntity.EndTime);

            var dto = new EventDetailDTO
            {
                Id = eventEntity.Id,
                GroupId = eventEntity.GroupId,
                GroupName = eventEntity.Group?.GroupName ?? "N/A",
                Title = eventEntity.Title,
                Content = eventEntity.Content,
                Banner = eventEntity.Banner,
                Images = string.IsNullOrEmpty(eventEntity.Images)
                    ? new List<string>()
                    : eventEntity.Images.Split(',').Select(s => s.Trim()).ToList(),
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                JoinCount = eventEntity.JoinCount,
                Type = eventEntity.Type,
                TypeText = eventEntity.Type == 1 ? "Nội bộ" : "Công khai",
                MeetingLink = eventEntity.MeetingLink,
                GoogleMapURL = eventEntity.GoogleMapURL,
                Address = eventEntity.Address,
                Status = currentStatus,
                StatusText = GetStatusText(currentStatus),
                StatusClass = GetStatusClass(currentStatus),
                IsActive = eventEntity.IsActive,
                CreatedDate = eventEntity.CreatedDate,
                UpdatedDate = eventEntity.UpdatedDate,
                NeedApproval = eventEntity.NeedApproval,
                Gifts = eventEntity.EventGifts?.Select(g =>
                {
                    Console.WriteLine($"Processing gift {g.Id} - Images: '{g.Images}'");
                    var images = string.IsNullOrEmpty(g.Images)
                        ? new List<string>()
                        : g.Images.Split(',').Select(s => s.Trim()).ToList();
                    Console.WriteLine($"Processed images count: {images.Count}");
                    return new EventGiftDTO
                    {
                        Id = g.Id,
                        EventId = g.EventId,
                        EventTitle = eventEntity.Title,
                        GiftName = g.GiftName,
                        Images = images,
                        Quantity = g.Quantity,
                        IsActive = true, // Default value since DB doesn't have this field
                        CreatedDate = g.CreatedDate,
                        UpdatedDate = g.UpdatedDate
                    };
                }).ToList() ?? new List<EventGiftDTO>(),
                // Registrations = eventEntity.EventRegistrations?.Select(r => new EventRegistrationDTO
                // {
                //     Id = r.Id,
                //     EventId = r.EventId,
                //     EventTitle = eventEntity.Title,
                //     UserZaloId = r.UserZaloId,
                //     Name = r.Name,
                //     PhoneNumber = r.PhoneNumber,
                //     Email = r.Email,
                //     CheckInCode = r.CheckInCode,
                //     Status = r.Status,
                //     StatusText = GetRegistrationStatusText(r.Status),
                //     StatusClass = GetRegistrationStatusClass(r.Status),
                //     CancelReason = r.CancelReason,
                //     CreatedDate = r.CreatedDate
                // }).ToList() ?? new List<EventRegistrationDTO>()
                Registrations = new List<EventRegistrationDTO>(), // Tạm thời trả về empty list
                Sponsors = eventEntity.EventSponsors?.Select(es => new EventSponsorDTO
                {
                    SponsorId = es.SponsorId,
                    SponsorName = es.Sponsor?.SponsorName ?? "N/A",
                    SponsorshipTierId = es.SponsorshipTierId,
                    SponsorshipTierName = es.SponsorshipTier?.TierName ?? "N/A"
                }).ToList() ?? new List<EventSponsorDTO>()
            };

            return dto;
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                1 => "Sắp diễn ra",
                2 => "Đang diễn ra",
                3 => "Đã kết thúc",
                _ => "Không xác định"
            };
        }

        private string GetStatusClass(int status)
        {
            return status switch
            {
                1 => "warning",
                2 => "success",
                3 => "secondary",
                _ => "secondary"
            };
        }

        private string GetRegistrationStatusText(int status)
        {
            return status switch
            {
                1 => "Chưa Tham Dự",
                2 => "Đã Tham Dự",
                3 => "Đã Hủy",
                _ => "Không Xác Định"
            };
        }

        private string GetRegistrationStatusClass(int status)
        {
            return status switch
            {
                1 => "badge bg-warning",
                2 => "badge bg-success",
                3 => "badge bg-danger",
                _ => "badge bg-secondary"
            };
        }

        private async Task ProcessEventGifts(string eventId, string giftsDataJson, List<IFormFile>? giftImages)
        {
            try
            {
                _logger.LogInformation("Processing event gifts for event {EventId}, giftsData: {GiftsData}, giftImages count: {GiftImagesCount}",
                    eventId, giftsDataJson, giftImages?.Count ?? 0);

                var giftsData = JsonSerializer.Deserialize<List<CreateEventGiftData>>(giftsDataJson);
                if (giftsData == null || !giftsData.Any())
                {
                    _logger.LogWarning("No gifts data found for event {EventId}", eventId);
                    return;
                }

                var giftImageUrls = new List<string>();

                // Process gift images if provided
                if (giftImages != null && giftImages.Any())
                {
                    _logger.LogInformation("Processing {Count} gift images for event {EventId}", giftImages.Count, eventId);
                    var savePath = Path.Combine(_env.WebRootPath, "uploads", "images", "gifts");

                    // Ensure directory exists
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    foreach (var giftImage in giftImages)
                    {
                        _logger.LogInformation("Processing gift image: {FileName}, Size: {Size}", giftImage.FileName, giftImage.Length);
                        var fileName = await FileHandler.SaveFile(giftImage, savePath);
                        var imageUrl = $"/uploads/images/gifts/{fileName}";
                        giftImageUrls.Add(imageUrl);
                        _logger.LogInformation("Gift image saved: {ImageUrl}", imageUrl);
                    }
                }
                else
                {
                    _logger.LogInformation("No gift images provided for event {EventId}", eventId);
                }

                // Create EventGift entities
                foreach (var giftData in giftsData)
                {
                    var eventGift = new EventGift
                    {
                        EventId = eventId,
                        GiftName = giftData.GiftName,
                        Quantity = giftData.Quantity,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    // Set image if available
                    if (giftData.ImageIndex.HasValue &&
                        giftData.ImageIndex.Value < giftImageUrls.Count)
                    {
                        eventGift.Images = giftImageUrls[giftData.ImageIndex.Value];
                    }

                    await _eventGiftRepository.AddAsync(eventGift);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event gifts for event {EventId}", eventId);
                throw;
            }
        }

        private async Task DeleteExistingEventGifts(string eventId)
        {
            try
            {
                _logger.LogInformation("Deleting existing gifts for event {EventId}", eventId);

                var existingGifts = await _eventGiftRepository.AsQueryable()
                    .Where(eg => eg.EventId == eventId)
                    .ToListAsync();

                foreach (var gift in existingGifts)
                {
                    // Delete gift images if exist
                    if (!string.IsNullOrEmpty(gift.Images))
                    {
                        var imagePath = Path.Combine(_env.WebRootPath, "uploads/images/gifts", gift.Images);
                        DeleteFile(imagePath);
                    }

                    _eventGiftRepository.Delete(gift);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} existing gifts for event {EventId}", existingGifts.Count, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting existing gifts for event {EventId}", eventId);
                throw;
            }
        }

        private async Task ProcessEventSponsor(string eventId, string sponsorId, string sponsorshipTierId)
        {
            try
            {
                var eventSponsor = new EventSponsor
                {
                    EventId = eventId,
                    SponsorId = sponsorId,
                    SponsorshipTierId = sponsorshipTierId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _eventSponsorRepository.AddAsync(eventSponsor);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event sponsor for event {EventId}", eventId);
                throw;
            }
        }

        private async Task UpdateEventSponsor(string eventId, string? sponsorId, string? sponsorshipTierId)
        {
            try
            {
                // Remove existing sponsors for this event
                var existingSponsors = await _eventSponsorRepository.AsQueryable()
                    .Where(es => es.EventId == eventId)
                    .ToListAsync();

                foreach (var existingSponsor in existingSponsors)
                {
                    _eventSponsorRepository.Delete(existingSponsor);
                }

                // Add new sponsor if provided
                if (!string.IsNullOrEmpty(sponsorId) && !string.IsNullOrEmpty(sponsorshipTierId))
                {
                    var eventSponsor = new EventSponsor
                    {
                        EventId = eventId,
                        SponsorId = sponsorId,
                        SponsorshipTierId = sponsorshipTierId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _eventSponsorRepository.AddAsync(eventSponsor);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event sponsor for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<object> GetEventCapacityInfoAsync(string eventId)
        {
            try
            {
                var eventEntity = await _eventRepository.AsQueryable()
                    .Where(e => e.Id == eventId)
                    .FirstOrDefaultAsync();

                if (eventEntity == null)
                {
                    throw new Exception("Không tìm thấy sự kiện");
                }

                // Đếm số người đã đăng ký tham gia (Status 1 = Registered, Status 2 = CheckedIn)
                var registeredCount = await _unitOfWork.GetRepository<EventRegistration>()
                    .AsQueryable()
                    .Where(er => er.EventId == eventId && (er.Status == (byte)ERegistrationStatus.Registered || er.Status == (byte)ERegistrationStatus.CheckedIn))
                    .CountAsync();

                // Đếm số khách mời đã đăng ký trực tiếp và đã được duyệt (Status 1 = Approved, Status 5 = Registered)
                var guestListRepository = _unitOfWork.GetRepository<GuestList>();
                var approvedGuestsCount = await guestListRepository
                    .AsQueryable()
                    .Where(gl => gl.EventGuest != null && 
                                 gl.EventGuest.EventId == eventId && 
                                 gl.Status == (byte)EGuestStatus.Approved)
                    .CountAsync();

                // Tổng số người tham gia = người đăng ký + khách mời đã duyệt
                var totalParticipants = registeredCount + approvedGuestsCount;

                var maxParticipants = eventEntity.JoinCount;
                var isUnlimited = maxParticipants == -1;
                var remainingSlots = isUnlimited ? -1 : Math.Max(0, maxParticipants - totalParticipants);
                var isFull = !isUnlimited && remainingSlots == 0;

                return new
                {
                    maxParticipants = maxParticipants,
                    registeredCount = registeredCount,
                    approvedGuestsCount = approvedGuestsCount,
                    totalParticipants = totalParticipants,
                    remainingSlots = remainingSlots,
                    isUnlimited = isUnlimited,
                    isFull = isFull
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event capacity info for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<EventStatisticsDTO> GetEventStatisticsAsync(string eventId)
        {
            try
            {
                var eventEntity = await _eventRepository.AsQueryable()
                    .Include(e => e.Group)
                    .Include(e => e.EventRegistrations)
                    .FirstOrDefaultAsync(e => e.Id == eventId);
                var guestList = await _guestListRepository.AsQueryable()
                    .Where(gl => gl.EventGuest != null && gl.EventGuest.EventId == eventId)
                    .ToListAsync();
                if (eventEntity == null)
                    throw new ArgumentException($"Event with id {eventId} not found");

                var registrations = eventEntity.EventRegistrations.ToList();

                var totalRegistrations = registrations.Count(r => r.Status != 3) + guestList.Count(gl => gl.Status != 3 && gl.Status == 1);
                var totalCheckedIn = registrations.Count(r => r.Status == 2) + guestList.Count(gl => gl.CheckInStatus == true && gl.Status != 3 && gl.Status == 1);
                var totalNotCheckedIn = registrations.Count(r => r.Status == 1) + guestList.Count(gl => gl.CheckInStatus == false && gl.Status != 3 && gl.Status == 1);
                var totalCancelled = registrations.Count(r => r.Status == 3) + guestList.Count(gl => gl.Status == 3);

                var attendanceRate = totalRegistrations > 0
                    ? Math.Round((double)totalCheckedIn / totalRegistrations * 100, 2)
                    : 0;

                // Lấy custom field values cho tất cả registrations và guests
                var registrationIds = registrations.Select(r => r.Id).ToList();
                var guestListIds = guestList.Select(gl => gl.Id).ToList();
                
                var customFieldValueRepository = _unitOfWork.GetRepository<EventCustomFieldValue>();
                var allCustomFieldValues = await customFieldValueRepository.AsQueryable()
                    .Where(cfv => 
                        (cfv.EventRegistrationId != null && registrationIds.Contains(cfv.EventRegistrationId)) ||
                        (cfv.GuestListId != null && guestListIds.Contains(cfv.GuestListId)))
                    .ToListAsync();

                // Lấy custom fields dựa trên EventCustomFieldId từ values (thay vì từ EventId)
                var customFieldIds = allCustomFieldValues.Select(cfv => cfv.EventCustomFieldId).Distinct().ToList();
                var customFields = await _eventCustomFieldRepository.AsQueryable()
                    .Where(cf => customFieldIds.Contains(cf.Id))
                    .ToListAsync();

                _logger.LogInformation("GetEventStatisticsAsync - EventId: {EventId}, CustomFieldIds from values: {FieldIds}, CustomFields count: {CustomFieldsCount}, CustomFieldValues count: {ValuesCount}",
                    eventId, string.Join(",", customFieldIds), customFields.Count, allCustomFieldValues.Count);

                // Tạo dictionary để map EventCustomFieldId -> FieldName (từ EventCustomField)
                var customFieldsDict = customFields.ToDictionary(cf => cf.Id, cf => cf.FieldName);

                // Helper function để lấy FieldName: ưu tiên từ value.FieldName, sau đó từ customFieldsDict, cuối cùng fallback sang ID
                string GetFieldName(EventCustomFieldValue cfv)
                {
                    // Ưu tiên 1: FieldName được lưu trực tiếp trong value
                    if (!string.IsNullOrEmpty(cfv.FieldName))
                        return cfv.FieldName;
                    // Ưu tiên 2: Lấy từ EventCustomField
                    if (customFieldsDict.TryGetValue(cfv.EventCustomFieldId, out var name))
                        return name;
                    // Fallback: Sử dụng ID
                    return $"Field_{cfv.EventCustomFieldId.Substring(0, Math.Min(8, cfv.EventCustomFieldId.Length))}";
                }

                // Lấy danh sách từ EventRegistrations
                var participants = registrations.Select(r => {
                    var customFieldDict = new Dictionary<string, string>();
                    var regCustomValues = allCustomFieldValues.Where(cfv => cfv.EventRegistrationId == r.Id).ToList();
                    foreach (var cfv in regCustomValues)
                    {
                        var fieldName = GetFieldName(cfv);
                        customFieldDict[fieldName] = cfv.FieldValue;
                    }
                    
                    return new EventParticipantDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        PhoneNumber = r.PhoneNumber,
                        Email = r.Email,
                        CheckInCode = r.CheckInCode,
                        RegisteredDate = r.CreatedDate,
                        CheckInTime = r.CheckInTime,
                        Status = r.Status,
                        StatusText = GetRegistrationStatusText(r.Status),
                        StatusClass = GetRegistrationStatusClass(r.Status),
                        ParticipantType = "Đăng ký",
                        CustomFieldValues = customFieldDict
                    };
                }).ToList();

                // Thêm khách mời từ GuestList (chỉ lấy những người đã được duyệt - Status == 1)
                var guestParticipants = guestList
                    .Where(gl => gl.Status == 1) // Chỉ lấy khách mời đã được duyệt
                    .Select(gl => {
                        var customFieldDict = new Dictionary<string, string>();
                        var guestCustomValues = allCustomFieldValues.Where(cfv => cfv.GuestListId == gl.Id).ToList();
                        
                        foreach (var cfv in guestCustomValues)
                        {
                            var fieldName = GetFieldName(cfv);
                            customFieldDict[fieldName] = cfv.FieldValue;
                        }
                        
                        return new EventParticipantDTO
                        {
                            Id = gl.Id,
                            Name = gl.GuestName ?? "Khách mời",
                            PhoneNumber = gl.GuestPhone,
                            Email = gl.GuestEmail,
                            CheckInCode = !string.IsNullOrEmpty(gl.CheckInCode) ? $"GUEST_{gl.CheckInCode}" : null,
                            RegisteredDate = gl.CreatedDate,
                            CheckInTime = gl.CheckInTime,
                            Status = gl.CheckInStatus == true ? 2 : (gl.Status == 3 ? 3 : 1), // Map to registration status
                            StatusText = gl.CheckInStatus == true ? "Đã Tham Dự" : (gl.Status == 3 ? "Đã Hủy" : "Chưa Tham Dự"),
                            StatusClass = gl.CheckInStatus == true ? "badge bg-success" : (gl.Status == 3 ? "badge bg-danger" : "badge bg-warning"),
                            ParticipantType = "Khách mời",
                            CustomFieldValues = customFieldDict
                        };
                    }).ToList();

                // Gộp 2 danh sách và sắp xếp
                participants.AddRange(guestParticipants);
                participants = participants.OrderByDescending(p => p.CheckInTime).ThenBy(p => p.Name).ToList();

                return new EventStatisticsDTO
                {
                    EventId = eventEntity.Id,
                    EventTitle = eventEntity.Title,
                    GroupName = eventEntity.Group?.GroupName ?? "N/A",
                    StartTime = eventEntity.StartTime,
                    EndTime = eventEntity.EndTime,
                    Address = eventEntity.Address,
                    MeetingLink = eventEntity.MeetingLink,
                    TotalRegistrations = totalRegistrations,
                    TotalCheckedIn = totalCheckedIn,
                    TotalNotCheckedIn = totalNotCheckedIn,
                    TotalCancelled = totalCancelled,
                    AttendanceRate = attendanceRate,
                    Participants = participants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event statistics for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<byte[]> ExportEventStatisticsAsync(string eventId)
        {
            try
            {
                var statistics = await GetEventStatisticsAsync(eventId);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var summarySheet = package.Workbook.Worksheets.Add("Thông Tin Sự Kiện");

                    summarySheet.Cells["A1"].Value = "BÁO CÁO SỰ KIỆN";
                    summarySheet.Cells["A1:D1"].Merge = true;
                    summarySheet.Cells["A1"].Style.Font.Size = 16;
                    summarySheet.Cells["A1"].Style.Font.Bold = true;
                    summarySheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int row = 3;
                    summarySheet.Cells[$"A{row}"].Value = "Tên Sự Kiện:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.EventTitle;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Hội Nhóm:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.GroupName;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Thời Gian:";
                    summarySheet.Cells[$"B{row}"].Value = $"{statistics.StartTime:dd/MM/yyyy HH:mm} - {statistics.EndTime:dd/MM/yyyy HH:mm}";
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Địa Điểm:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.Address ?? "N/A";
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;

                    row += 2;
                    summarySheet.Cells[$"A{row}"].Value = "THỐNG KÊ TỔNG QUAN";
                    summarySheet.Cells[$"A{row}:D{row}"].Merge = true;
                    summarySheet.Cells[$"A{row}"].Style.Font.Size = 14;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                    summarySheet.Cells[$"A{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Tổng Đăng Ký:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.TotalRegistrations;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Đã Tham Dự:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.TotalCheckedIn;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                    summarySheet.Cells[$"B{row}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Chưa Tham Dự:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.TotalNotCheckedIn;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                    summarySheet.Cells[$"B{row}"].Style.Font.Color.SetColor(System.Drawing.Color.Orange);

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Đã Hủy:";
                    summarySheet.Cells[$"B{row}"].Value = statistics.TotalCancelled;
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                    summarySheet.Cells[$"B{row}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    row++;
                    summarySheet.Cells[$"A{row}"].Value = "Tỷ Lệ Tham Dự:";
                    summarySheet.Cells[$"B{row}"].Value = $"{statistics.AttendanceRate}%";
                    summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                    summarySheet.Cells[$"B{row}"].Style.Font.Color.SetColor(System.Drawing.Color.Blue);

                    summarySheet.Column(1).Width = 20;
                    summarySheet.Column(2).Width = 40;

                    var detailSheet = package.Workbook.Worksheets.Add("Danh Sách Người Tham Gia");

                    // Lấy danh sách tất cả custom field names từ participants
                    var allCustomFieldNames = statistics.Participants
                        .SelectMany(p => p.CustomFieldValues.Keys)
                        .Distinct()
                        .OrderBy(k => k)
                        .ToList();

                    // Header columns cố định
                    int col = 1;
                    detailSheet.Cells[1, col++].Value = "STT";
                    detailSheet.Cells[1, col++].Value = "Loại";
                    detailSheet.Cells[1, col++].Value = "Họ Tên";
                    detailSheet.Cells[1, col++].Value = "Số Điện Thoại";
                    detailSheet.Cells[1, col++].Value = "Email";
                    detailSheet.Cells[1, col++].Value = "Mã Check-in";
                    detailSheet.Cells[1, col++].Value = "Ngày Đăng Ký";
                    detailSheet.Cells[1, col++].Value = "Thời Gian Check-in";
                    detailSheet.Cells[1, col++].Value = "Trạng Thái";

                    // Thêm header cho custom fields
                    int customFieldStartCol = col;
                    foreach (var fieldName in allCustomFieldNames)
                    {
                        detailSheet.Cells[1, col++].Value = fieldName;
                    }

                    int totalCols = col - 1;

                    // Style header
                    var headerRange = detailSheet.Cells[1, 1, 1, totalCols];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int dataRow = 2;
                    int stt = 1;
                    foreach (var participant in statistics.Participants)
                    {
                        col = 1;
                        detailSheet.Cells[dataRow, col++].Value = stt++;
                        detailSheet.Cells[dataRow, col++].Value = participant.ParticipantType;
                        detailSheet.Cells[dataRow, col++].Value = participant.Name;
                        detailSheet.Cells[dataRow, col++].Value = participant.PhoneNumber ?? "";
                        detailSheet.Cells[dataRow, col++].Value = participant.Email ?? "";
                        detailSheet.Cells[dataRow, col++].Value = participant.CheckInCode ?? "";
                        detailSheet.Cells[dataRow, col++].Value = participant.RegisteredDate.ToString("dd/MM/yyyy HH:mm");
                        detailSheet.Cells[dataRow, col++].Value = participant.CheckInTime.HasValue
                            ? participant.CheckInTime.Value.ToString("dd/MM/yyyy HH:mm")
                            : "";
                        
                        var statusCell = detailSheet.Cells[dataRow, col++];
                        statusCell.Value = participant.StatusText;
                        if (participant.Status == 2)
                            statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        else if (participant.Status == 3)
                            statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        else
                            statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Orange);

                        // Thêm giá trị custom fields
                        foreach (var fieldName in allCustomFieldNames)
                        {
                            var value = participant.CustomFieldValues.TryGetValue(fieldName, out var v) ? v : "";
                            detailSheet.Cells[dataRow, col++].Value = value;
                        }

                        dataRow++;
                    }

                    // Auto-fit columns only if there's data
                    if (detailSheet.Dimension != null)
                    {
                        detailSheet.Cells[detailSheet.Dimension.Address].AutoFitColumns();
                    }
                    else
                    {
                        // Set default column widths if no data
                        detailSheet.Column(1).Width = 10;  // STT
                        detailSheet.Column(2).Width = 15;  // Loại
                        detailSheet.Column(3).Width = 30;  // Họ Tên
                        detailSheet.Column(4).Width = 20;  // Số Điện Thoại
                        detailSheet.Column(5).Width = 30;  // Email
                        detailSheet.Column(6).Width = 20;  // Mã Check-in
                        detailSheet.Column(7).Width = 20;  // Ngày Đăng Ký
                        detailSheet.Column(8).Width = 20;  // Thời Gian Check-in
                        detailSheet.Column(9).Width = 20;  // Trạng Thái
                    }

                    using (var stream = new MemoryStream())
                    {
                        await package.SaveAsAsync(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting event statistics for event {EventId}", eventId);
                throw;
            }
        }
        private async Task ProcessEventCustomFields(string eventId, string customFieldsDataJson)
        {
            try
            {
                _logger.LogInformation("Processing event custom fields for event {EventId}, customFieldsData: {CustomFieldsData}",
                    eventId, customFieldsDataJson);

                var customFieldsData = JsonSerializer.Deserialize<List<CreateEventCustomFieldData>>(customFieldsDataJson);
                if (customFieldsData == null || !customFieldsData.Any())
                {
                    _logger.LogWarning("No custom fields data found for event {EventId}", eventId);
                    return;
                }

                // Create EventCustomField entities
                foreach (var fieldData in customFieldsData)
                {
                    var eventCustomField = new EventCustomField
                    {
                        EventId = eventId,
                        FieldName = fieldData.FieldName,
                        FieldValue = fieldData.FieldValue ?? string.Empty,
                        FieldType = fieldData.FieldType,
                        IsRequired = fieldData.IsRequired,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _eventCustomFieldRepository.AddAsync(eventCustomField);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event custom fields for event {EventId}", eventId);
                throw;
            }
        }

        private async Task DeleteExistingEventCustomFields(string eventId)
        {
            try
            {
                _logger.LogInformation("Deleting existing custom fields for event {EventId}", eventId);

                var existingFields = await _eventCustomFieldRepository.AsQueryable()
                    .Where(f => f.EventId == eventId)
                    .ToListAsync();

                if (existingFields.Any())
                {
                    foreach (var field in existingFields)
                    {
                        _eventCustomFieldRepository.Delete(field);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Deleted {Count} custom fields for event {EventId}", existingFields.Count, eventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting existing custom fields for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<EventDTO>> GuestGetEvent()
        {
            var events = await _eventRepository.AsQueryable()
                .Where(e => e.IsActive && e.Type == 2 && e.StartTime > DateTime.Now)
                .ToListAsync();
            return events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                IsActive = e.IsActive
            }).ToList();
        }
        #endregion
    }


}
