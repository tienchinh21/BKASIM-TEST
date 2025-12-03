using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helpers;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Features.EventTrigger;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Payload;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.Events;
using MiniAppGIBA.Services.Groups;
using MiniAppGIBA.Models.Response.Events;
using MiniAppGIBA.Enum;


namespace MiniAppGIBA.Services.EventGuests
{
    public class EventGuestService : IEventGuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EventGuest> _eventGuestRepository;
        private readonly IRepository<GuestList> _guestListRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<EventRegistration> _eventRegistrationRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<EventGuestService> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IUrl _url;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventGuestService(
            IUnitOfWork unitOfWork,
            IRepository<EventGuest> eventGuestRepository,
            IRepository<GuestList> guestListRepository,
            IRepository<Event> eventRepository,
            IRepository<EventRegistration> eventRegistrationRepository,
            IEventService eventService,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<EventGuestService> logger,
            IUrl url,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _eventGuestRepository = eventGuestRepository;
            _guestListRepository = guestListRepository;
            _eventRepository = eventRepository;
            _eventRegistrationRepository = eventRegistrationRepository;
            _eventService = eventService;
            _logger = logger;
            _mediator = mediator;
            _configuration = configuration;
            _url = url;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EventGuestDTO> RegisterGuestListAsync(string eventId, string userZaloId, RegisterGuestListRequest request)
        {
            try
            {
                // 1. Kiểm tra sự kiện có tồn tại không
                var eventEntity = await _eventRepository.AsQueryable()
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                {
                    throw new Exception("Không tìm thấy sự kiện");
                }

                if (!eventEntity.IsActive)
                {
                    throw new Exception("Sự kiện đã ngừng hoạt động");
                }

                // 2. Validation dữ liệu đầu vào
                if (request.GuestList == null || request.GuestList.Count == 0)
                {
                    throw new Exception("Danh sách khách mời không được để trống");
                }

                // Kiểm tra GuestNumber có khớp với số lượng thực tế không
                if (request.GuestNumber != request.GuestList.Count)
                {
                    throw new Exception($"Số lượng khách mời ({request.GuestNumber}) không khớp với danh sách thực tế ({request.GuestList.Count})");
                }

                var guestNumber = request.GuestNumber; // Sử dụng số lượng user nhập
                _logger.LogInformation("User {UserZaloId} đăng ký {GuestNumber} khách mời cho sự kiện {EventId}", userZaloId, guestNumber, eventId);

                // 3. Kiểm tra capacity nếu sự kiện có giới hạn
                if (eventEntity.JoinCount > 0) // Có giới hạn
                {
                    // Đếm số người đã đăng ký và đã được duyệt (EventRegistration, status = 1 hoặc 2, không tính 0 = pending và 3 = cancelled)
                    var registeredCount = await _eventRegistrationRepository.AsQueryable()
                        .Where(er => er.EventId == eventId && (er.Status == 1 || er.Status == 2))
                        .CountAsync();

                    // Đếm số khách đã được duyệt (EventGuest, status = 1 = đã duyệt)
                    var approvedGuestsCount = await _eventGuestRepository.AsQueryable()
                        .Where(eg => eg.EventId == eventId && eg.Status == 1)
                        .SumAsync(eg => eg.GuestNumber);

                    var totalParticipants = registeredCount + approvedGuestsCount;
                    var remainingSlots = eventEntity.JoinCount - totalParticipants;

                    _logger.LogInformation(
                        "Event capacity check: EventId={EventId}, MaxCapacity={MaxCapacity}, Registered={Registered}, ApprovedGuests={ApprovedGuests}, Total={Total}, Remaining={Remaining}, RequestedGuests={RequestedGuests}",
                        eventId, eventEntity.JoinCount, registeredCount, approvedGuestsCount, totalParticipants, remainingSlots, guestNumber);

                    // Kiểm tra nếu số khách mời vượt quá slot còn lại
                    if (guestNumber > remainingSlots)
                    {
                        throw new Exception($"Sự kiện chỉ còn {remainingSlots} chỗ trống. Bạn không thể đăng ký {guestNumber} khách mời.");
                    }
                }

                // 4. Tạo EventGuest
                var eventGuest = new EventGuest
                {
                    EventId = eventId,
                    UserZaloId = userZaloId,
                    Note = request.Note,
                    GuestNumber = guestNumber, // Số lượng khách mời thực tế
                    Status = eventEntity.NeedApproval ? (byte)EGuestStatus.Pending : (byte)EGuestStatus.Approved, // Pending or Approved
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _eventGuestRepository.AddAsync(eventGuest);
                await _unitOfWork.SaveChangesAsync();

                // 5. Tạo GuestList cho từng khách
                foreach (var guest in request.GuestList)
                {
                    var guestList = new GuestList
                    {
                        EventGuestId = eventGuest.Id,
                        GuestName = guest.GuestName,
                        GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(guest.GuestPhone ?? string.Empty),
                        GuestEmail = guest.GuestEmail,
                        Status = 0, // Chờ xử lý (sẽ được admin duyệt/từ chối)
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _guestListRepository.AddAsync(guestList);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Đã tạo EventGuest {EventGuestId} với {GuestNumber} khách mời cho user {UserZaloId}", eventGuest.Id, guestNumber, userZaloId);

                // 6. Load lại với GuestLists
                var result = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuest.Id)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                return MapToDTO(result!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering guest list for event {EventId} by user {UserZaloId}", eventId, userZaloId);
                throw;
            }
        }

        public async Task<EventGuestDTO> UpdateGuestListAsync(string eventGuestId, string userZaloId, RegisterGuestListRequest request)
        {
            try
            {
                // 1. Kiểm tra đơn đăng ký tồn tại và thuộc về user
                var eventGuest = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuestId && eg.UserZaloId == userZaloId)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                if (eventGuest == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký khách mời");
                }

                // 2. Kiểm tra trạng thái - chỉ cho phép cập nhật khi chưa được duyệt
                if (eventGuest.Status != 0) // 0 = Chờ xử lý
                {
                    throw new Exception("Chỉ có thể cập nhật đơn đăng ký khi chưa được duyệt");
                }

                // 3. Kiểm tra capacity
                var eventCapacity = await _eventService.GetEventCapacityInfoAsync(eventGuest.EventId);
                var newGuestNumber = request.GuestList.Count;
                var currentApprovedGuests = ((dynamic)eventCapacity).ApprovedGuestsCount;
                var availableSlots = ((dynamic)eventCapacity).Capacity - currentApprovedGuests;

                if (newGuestNumber > availableSlots)
                {
                    throw new Exception($"Số lượng khách mời ({newGuestNumber}) vượt quá số slot còn lại ({availableSlots})");
                }

                // 4. Xóa danh sách khách mời cũ
                foreach (var guest in eventGuest.GuestLists)
                {
                    _guestListRepository.Delete(guest);
                }

                // 5. Cập nhật thông tin đơn đăng ký
                eventGuest.Note = request.Note;
                eventGuest.GuestNumber = newGuestNumber;
                eventGuest.UpdatedDate = DateTime.Now;

                _eventGuestRepository.Update(eventGuest);

                // 6. Tạo danh sách khách mời mới
                foreach (var guest in request.GuestList)
                {
                    var guestList = new GuestList
                    {
                        EventGuestId = eventGuest.Id,
                        GuestName = guest.GuestName,
                        GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(guest.GuestPhone ?? string.Empty),
                        GuestEmail = guest.GuestEmail,
                        Status = 0, // Chờ xử lý
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _guestListRepository.AddAsync(guestList);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật EventGuest {EventGuestId} với {GuestNumber} khách mời cho user {UserZaloId}", eventGuest.Id, newGuestNumber, userZaloId);

                // 7. Load lại với GuestLists
                var result = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuest.Id)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                return MapToDTO(result!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guest list {EventGuestId} by user {UserZaloId}", eventGuestId, userZaloId);
                throw;
            }
        }

        public async Task<EventGuestDTO> ApproveGuestListAsync(string eventGuestId, ApproveGuestListRequest request)
        {
            try
            {
                // Kiểm tra trong EventGuest trước
                var eventGuest = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuestId)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                bool isEventRegistration = false;
                EventRegistration? eventRegistration = null;

                // Nếu không tìm thấy trong EventGuest, kiểm tra trong EventRegistration (đăng ký đơn lẻ)
                if (eventGuest == null)
                {
                    eventRegistration = await _eventRegistrationRepository.AsQueryable()
                        .Where(er => er.Id == eventGuestId)
                        .Include(er => er.Event)
                        .Include(er => er.Membership)
                        .FirstOrDefaultAsync();

                    if (eventRegistration == null)
                    {
                        throw new Exception("Không tìm thấy đơn đăng ký khách mời");
                    }

                    isEventRegistration = true;

                    // Kiểm tra status cho EventRegistration
                    if (eventRegistration.Status != 0) // 1 = Pending
                    {
                        throw new Exception("Chỉ có thể duyệt đơn đang ở trạng thái chờ xử lý");
                    }
                }
                else
                {
                    if (eventGuest.Status != 0)
                    {
                        throw new Exception("Chỉ có thể duyệt đơn đang ở trạng thái chờ xử lý");
                    }
                }

                if (isEventRegistration)
                {
                    // Xử lý EventRegistration (đăng ký đơn lẻ)
                    if (eventRegistration == null || eventRegistration.Event == null)
                    {
                        throw new Exception("Không tìm thấy thông tin sự kiện");
                    }
                    var eventEntity = eventRegistration.Event;

                    if (request.IsApproved)
                    {
                        // Kiểm tra lại capacity trước khi duyệt
                        if (eventEntity.JoinCount > 0)
                        {
                            var registeredCount = await _eventRegistrationRepository.AsQueryable()
                                .Where(er => er.EventId == eventRegistration.EventId && (er.Status == 1 || er.Status == 2) && er.Id != eventRegistration.Id)
                                .CountAsync();

                            var approvedGuestsCount = await _eventGuestRepository.AsQueryable()
                                .Where(eg => eg.EventId == eventRegistration.EventId && eg.Status == 1)
                                .SumAsync(eg => eg.GuestNumber);

                            var totalParticipants = registeredCount + approvedGuestsCount;
                            var remainingSlots = eventEntity.JoinCount - totalParticipants;

                            if (remainingSlots < 1)
                            {
                                throw new Exception($"Sự kiện chỉ còn {remainingSlots} chỗ trống. Không thể duyệt đăng ký này.");
                            }
                        }

                        // Tạo check-in code nếu chưa có
                        if (string.IsNullOrEmpty(eventRegistration.CheckInCode))
                        {
                            string? checkInCode;
                            do
                            {
                                checkInCode = GenerateRandomCode(8);
                            } while (await _eventRegistrationRepository.AsQueryable()
                                .AnyAsync(er => er.CheckInCode == checkInCode));
                            eventRegistration.CheckInCode = checkInCode;
                        }

                        eventRegistration.Status = 1; // Đã xác nhận
                    }
                    else
                    {
                        // EventRegistration không có status "Từ chối" riêng
                        // Dùng Cancelled (3) và ghi lý do từ chối vào CancelReason
                        // Khi convert sang EventGuestDTO sẽ hiển thị là "Từ chối" (status 2) thay vì "Hủy" (status 3)
                        eventRegistration.Status = 3; // Cancelled (nhưng sẽ hiển thị là "Từ chối" khi convert)
                        eventRegistration.CancelReason = request.RejectReason;
                    }

                    eventRegistration.UpdatedDate = DateTime.Now;
                    _eventRegistrationRepository.Update(eventRegistration);
                    await _unitOfWork.SaveChangesAsync();

                    // Convert EventRegistration thành EventGuestDTO để trả về
                    // Nếu là từ chối (request.IsApproved = false), convert status 3 thành 2 (Từ chối)
                    bool isRejected = !request.IsApproved && eventRegistration.Status == 3;
                    byte guestStatus = ConvertRegistrationStatusToGuestStatus(eventRegistration.Status, eventRegistration.CancelReason, isRejected);
                    string statusText = isRejected ? "Từ chối" : GetRegistrationStatusText(eventRegistration.Status);

                    return new EventGuestDTO
                    {
                        Id = eventRegistration.Id,
                        EventId = eventRegistration.EventId,
                        EventTitle = eventRegistration.Event?.Title ?? "N/A",
                        UserZaloId = eventRegistration.UserZaloId,
                        Note = "Đăng ký đơn lẻ",
                        GuestNumber = 1,
                        Status = guestStatus,
                        StatusText = statusText,
                        CancelReason = eventRegistration.CancelReason,
                        CreatedDate = eventRegistration.CreatedDate,
                        GuestLists = new List<GuestListDTO>
                        {
                            new GuestListDTO
                            {
                                Id = eventRegistration.Id,
                                EventGuestId = eventRegistration.Id,
                                GuestName = eventRegistration.Name,
                                GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(eventRegistration.PhoneNumber ?? string.Empty),
                                GuestEmail = eventRegistration.Email,
                                Status = guestStatus,
                                StatusText = statusText
                            }
                        },
                        MemberName = eventRegistration.Membership?.Fullname ?? eventRegistration.Name,
                        MemberPhone = eventRegistration.Membership?.PhoneNumber ?? eventRegistration.PhoneNumber,
                        MemberEmail = eventRegistration.Email,
                        MemberCompany = null,
                        MemberPosition = null
                    };
                }
                else
                {
                    // Xử lý EventGuest (đăng ký theo nhóm)
                    if (request.IsApproved)
                    {
                        // Kiểm tra lại capacity trước khi duyệt
                        if (eventGuest!.Event!.JoinCount > 0)
                        {
                            var registeredCount = await _eventRegistrationRepository.AsQueryable()
                                .Where(er => er.EventId == eventGuest.EventId && (er.Status == 1 || er.Status == 2))
                                .CountAsync();

                            var approvedGuestsCount = await _eventGuestRepository.AsQueryable()
                                .Where(eg => eg.EventId == eventGuest.EventId && eg.Status == 1 && eg.Id != eventGuestId)
                                .SumAsync(eg => eg.GuestNumber);

                            var totalParticipants = registeredCount + approvedGuestsCount;
                            var remainingSlots = eventGuest.Event.JoinCount - totalParticipants;

                            if (eventGuest.GuestNumber > remainingSlots)
                            {
                                throw new Exception($"Sự kiện chỉ còn {remainingSlots} chỗ trống. Không thể duyệt {eventGuest.GuestNumber} khách mời.");
                            }
                        }

                        eventGuest.Status = 1; // Đã duyệt



                    }
                    else
                    {
                        if (eventGuest == null)
                        {
                            throw new Exception("Không tìm thấy đơn đăng ký");
                        }

                        eventGuest.Status = 2; // Từ chối
                        eventGuest.RejectReason = request.RejectReason;

                        // Cập nhật status cho tất cả GuestList
                        foreach (var guest in eventGuest.GuestLists)
                        {
                            guest.Status = 2;
                            guest.UpdatedDate = DateTime.Now;
                            _guestListRepository.Update(guest);
                        }
                    }

                    eventGuest.UpdatedDate = DateTime.Now;
                    _eventGuestRepository.Update(eventGuest);
                    await _unitOfWork.SaveChangesAsync();

                    // Load lại với đầy đủ thông tin trước khi map DTO và trigger event
                    var updatedEventGuest = await _eventGuestRepository.AsQueryable()
                        .Where(eg => eg.Id == eventGuest.Id)
                        .Include(eg => eg.Event)
                        .Include(eg => eg.Membership)
                        .Include(eg => eg.GuestLists)
                        .FirstOrDefaultAsync();

                    if (updatedEventGuest == null)
                    {
                        throw new Exception("Không tìm thấy đơn đăng ký sau khi cập nhật");
                    }

                    if (request.IsApproved)
                    {
                        var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                        var payload = CreateEventPayloadFromData(updatedEventGuest);
                        BackgroundJob.Schedule<EventGuestService>(
                            x => x.TriggerEventGuestEvent("Membership.UpdateEventStatus", payload),
                            TimeSpan.FromSeconds(delayInSeconds)
                        );
                    }

                    return MapToDTO(updatedEventGuest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving guest list {EventGuestId}", eventGuestId);
                throw;
            }
        }

        public async Task<EventGuestDTO> CancelGuestListAsync(string eventGuestId, string cancelReason)
        {
            try
            {
                var eventGuest = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuestId)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                if (eventGuest == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký khách mời");
                }

                if (eventGuest.Status == 3)
                {
                    throw new Exception("Đơn đã được hủy trước đó");
                }

                eventGuest.Status = 3; // Hủy
                eventGuest.CancelReason = cancelReason;
                eventGuest.UpdatedDate = DateTime.Now;

                // Cập nhật status cho tất cả GuestList
                foreach (var guest in eventGuest.GuestLists)
                {
                    guest.Status = 3;
                    guest.UpdatedDate = DateTime.Now;
                    _guestListRepository.Update(guest);
                }

                _eventGuestRepository.Update(eventGuest);
                var result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                    var payload = CreateEventPayloadFromData(eventGuest);
                    BackgroundJob.Schedule<EventGuestService>(
                        x => x.TriggerEventGuestEvent("Membership.UpdateEventStatus", payload),
                        TimeSpan.FromSeconds(delayInSeconds)
                    );
                }

                return MapToDTO(eventGuest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling guest list {EventGuestId}", eventGuestId);
                throw;
            }
        }

        public async Task<EventGuestDTO?> GetEventGuestByIdAsync(string eventGuestId)
        {
            try
            {
                // Kiểm tra trong EventGuest trước
                var eventGuest = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuestId)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                if (eventGuest != null)
                {
                    return MapToDTO(eventGuest);
                }

                // Nếu không tìm thấy trong EventGuest, kiểm tra trong EventRegistration (đăng ký đơn lẻ)
                var eventRegistration = await _eventRegistrationRepository.AsQueryable()
                    .Where(er => er.Id == eventGuestId)
                    .Include(er => er.Event)
                    .Include(er => er.Membership)
                    .FirstOrDefaultAsync();

                if (eventRegistration != null)
                {
                    // Convert EventRegistration thành EventGuestDTO format
                    // Xử lý trường hợp từ chối: nếu Status = 3 và có CancelReason, có thể là từ chối (status 2) hoặc hủy (status 3)
                    // Heuristic: Nếu Status = 3 và có CancelReason, coi là "Từ chối" (status 2) thay vì "Hủy" (status 3)
                    // Vì khi user tự hủy thường không có CancelReason, còn khi admin từ chối thì có CancelReason
                    bool isRejected = eventRegistration.Status == 3 && !string.IsNullOrEmpty(eventRegistration.CancelReason);
                    byte guestStatus = ConvertRegistrationStatusToGuestStatus(eventRegistration.Status, eventRegistration.CancelReason, isRejected);
                    string statusText = isRejected ? "Từ chối" : GetRegistrationStatusText(eventRegistration.Status);

                    return new EventGuestDTO
                    {
                        Id = eventRegistration.Id,
                        EventId = eventRegistration.EventId,
                        EventTitle = eventRegistration.Event?.Title ?? "N/A",
                        UserZaloId = eventRegistration.UserZaloId,
                        Note = "Đăng ký đơn lẻ",
                        GuestNumber = 1,
                        Status = guestStatus,
                        StatusText = statusText,
                        CancelReason = eventRegistration.CancelReason,
                        CreatedDate = eventRegistration.CreatedDate,
                        GuestLists = new List<GuestListDTO>
                        {
                            new GuestListDTO
                            {
                                Id = eventRegistration.Id,
                                EventGuestId = eventRegistration.Id,
                                GuestName = eventRegistration.Name,
                                GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(eventRegistration.PhoneNumber ?? string.Empty),
                                GuestEmail = eventRegistration.Email,
                                Status = guestStatus,
                                StatusText = statusText
                            }
                        },
                        MemberName = eventRegistration.Membership?.Fullname ?? eventRegistration.Name,
                        MemberPhone = eventRegistration.Membership?.PhoneNumber ?? eventRegistration.PhoneNumber,
                        MemberEmail = eventRegistration.Email,
                        MemberCompany = null,
                        MemberPosition = null
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event guest {EventGuestId}", eventGuestId);
                throw;
            }
        }

        public async Task<List<EventGuestDTO>> GetMyGuestListsAsync(string userZaloId)
        {
            try
            {
                var eventGuests = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.UserZaloId == userZaloId)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .OrderByDescending(eg => eg.CreatedDate)
                    .ToListAsync();

                return eventGuests.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<PagedResult<EventGuestDTO>> GetMyGuestListsAsync(string userZaloId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null)
        {
            try
            {
                var baseQuery = _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.UserZaloId == userZaloId);

                // Filter by keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    baseQuery = baseQuery.Where(eg => eg.Note!.Contains(keyword));
                }

                // Filter by status
                if (status.HasValue)
                {
                    baseQuery = baseQuery.Where(eg => eg.Status == status.Value);
                }

                var query = baseQuery
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists);

                var totalCount = await query.CountAsync();

                var eventGuests = await query
                    .OrderByDescending(eg => eg.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = eventGuests.Select(MapToDTO).ToList();

                return new PagedResult<EventGuestDTO>
                {
                    Items = items,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<List<EventGuestDTO>> GetEventGuestListsAsync(string eventId)
        {
            try
            {
                var eventGuests = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.EventId == eventId)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .OrderByDescending(eg => eg.CreatedDate)
                    .ToListAsync();

                return eventGuests.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<PagedResult<EventGuestDTO>> GetEventGuestListsAsync(string eventId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null)
        {
            try
            {
                var baseQuery = _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.EventId == eventId);

                // Filter by keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    baseQuery = baseQuery.Where(eg => eg.Note!.Contains(keyword) ||
                                           eg.UserZaloId.Contains(keyword));
                }

                // Filter by status
                if (status.HasValue)
                {
                    baseQuery = baseQuery.Where(eg => eg.Status == status.Value);
                }

                var query = baseQuery
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists);

                var totalCount = await query.CountAsync();

                var eventGuests = await query
                    .OrderByDescending(eg => eg.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = eventGuests.Select(MapToDTO).ToList();

                return new PagedResult<EventGuestDTO>
                {
                    Items = items,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<EventGuestDTO>> GetAllEventGuestsAsync()
        {
            try
            {
                var eventGuests = await _eventGuestRepository.AsQueryable()
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .OrderByDescending(eg => eg.CreatedDate)
                    .ToListAsync();

                return eventGuests.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all event guests");
                throw;
            }
        }

        public async Task<List<EventGuestDTO>> GetEventGuestListsWithRegistrationsAsync(string eventId, List<string>? allowedGroupIds = null, string? groupType = null)
        {
            try
            {
                var result = new List<EventGuestDTO>();

                // Build query với filter theo group permissions
                IQueryable<EventGuest> eventGuestsQuery = _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.EventId == eventId)
                    .Include(eg => eg.Event)
                        .ThenInclude(e => e.Group)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists);

                // Apply group filter sau khi Include
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var allowedGroupIdsList = allowedGroupIds.ToList();
                    eventGuestsQuery = eventGuestsQuery
                        .Where(eg => allowedGroupIdsList.Contains(eg.Event.GroupId));
                }
                var eventGuests = await eventGuestsQuery
                    .OrderByDescending(eg => eg.CreatedDate)
                    .ToListAsync();

                result.AddRange(eventGuests.Select(MapToDTO));

                // Lấy EventRegistration (đăng ký đơn riêng lẻ)
                IQueryable<EventRegistration> eventRegistrationsQuery = _eventRegistrationRepository.AsQueryable()
                    .Where(er => er.EventId == eventId)
                    .Include(er => er.Event)
                        .ThenInclude(e => e.Group)
                    .Include(er => er.Membership);

                // Apply group filter cho EventRegistration sau khi Include
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var allowedGroupIdsList = allowedGroupIds.ToList();
                    eventRegistrationsQuery = eventRegistrationsQuery
                        .Where(er => allowedGroupIdsList.Contains(er.Event.GroupId));
                }

                var eventRegistrations = await eventRegistrationsQuery
                    .OrderByDescending(er => er.CreatedDate)
                    .ToListAsync();

                foreach (var registration in eventRegistrations)
                {
                    // Xử lý trường hợp từ chối: nếu Status = 3 và có CancelReason, coi là "Từ chối"
                    bool isRejected = registration.Status == 3 && !string.IsNullOrEmpty(registration.CancelReason);
                    byte guestStatus = ConvertRegistrationStatusToGuestStatus(registration.Status, registration.CancelReason, isRejected);
                    string statusText = isRejected ? "Từ chối" : GetRegistrationStatusText(registration.Status);

                    var guestDTO = new EventGuestDTO
                    {
                        Id = registration.Id,
                        EventId = registration.EventId,
                        EventTitle = registration.Event?.Title ?? "N/A",
                        UserZaloId = registration.UserZaloId,
                        Note = "Đăng ký đơn lẻ",
                        GuestNumber = 1,
                        Status = guestStatus,
                        StatusText = statusText,
                        CancelReason = registration.CancelReason,
                        CreatedDate = registration.CreatedDate,
                        GuestLists = new List<GuestListDTO>
                        {
                            new GuestListDTO
                            {
                                Id = registration.Id,
                                EventGuestId = registration.Id,
                                GuestName = registration.Name,
                                GuestPhone = registration.PhoneNumber,
                                GuestEmail = registration.Email,
                                Status = guestStatus,
                                StatusText = statusText
                            }
                        },
                        MemberName = registration.Membership?.Fullname ?? registration.Name,
                        MemberPhone = registration.Membership?.PhoneNumber ?? registration.PhoneNumber,
                        MemberEmail = registration.Email,
                        MemberCompany = null,
                        MemberPosition = null
                    };
                    result.Add(guestDTO);
                }

                return result.OrderByDescending(x => x.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest lists with registrations for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<EventGuestDTO>> GetAllEventGuestsWithRegistrationsAsync(List<string>? allowedGroupIds = null, string? groupType = null)
        {
            try
            {
                var result = new List<EventGuestDTO>();

                // Build query với filter theo group permissions
                IQueryable<EventGuest> eventGuestsQuery = _eventGuestRepository.AsQueryable()
                    .Include(eg => eg.Event)
                        .ThenInclude(e => e.Group)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists);

                // Apply group filter sau khi Include
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var allowedGroupIdsList = allowedGroupIds.ToList();
                    eventGuestsQuery = eventGuestsQuery
                        .Where(eg => allowedGroupIdsList.Contains(eg.Event.GroupId));
                }

                var eventGuests = await eventGuestsQuery
                    .OrderByDescending(eg => eg.CreatedDate)
                    .ToListAsync();

                result.AddRange(eventGuests.Select(MapToDTO));

                // Lấy EventRegistration (đăng ký đơn riêng lẻ)
                IQueryable<EventRegistration> eventRegistrationsQuery = _eventRegistrationRepository.AsQueryable()
                    .Include(er => er.Event)
                        .ThenInclude(e => e.Group)
                    .Include(er => er.Membership);

                // Apply group filter cho EventRegistration sau khi Include
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var allowedGroupIdsList = allowedGroupIds.ToList();
                    eventRegistrationsQuery = eventRegistrationsQuery
                        .Where(er => allowedGroupIdsList.Contains(er.Event.GroupId));
                }

                var eventRegistrations = await eventRegistrationsQuery
                    .OrderByDescending(er => er.CreatedDate)
                    .ToListAsync();

                // Convert EventRegistration thành EventGuestDTO format
                foreach (var registration in eventRegistrations)
                {
                    // Xử lý trường hợp từ chối: nếu Status = 3 và có CancelReason, coi là "Từ chối"
                    bool isRejected = registration.Status == 3 && !string.IsNullOrEmpty(registration.CancelReason);
                    byte guestStatus = ConvertRegistrationStatusToGuestStatus(registration.Status, registration.CancelReason, isRejected);
                    string statusText = isRejected ? "Từ chối" : GetRegistrationStatusText(registration.Status);

                    var guestDTO = new EventGuestDTO
                    {
                        Id = registration.Id,
                        EventId = registration.EventId,
                        EventTitle = registration.Event?.Title ?? "N/A",
                        UserZaloId = registration.UserZaloId,
                        Note = "Đăng ký đơn lẻ",
                        GuestNumber = 1,
                        Status = guestStatus,
                        StatusText = statusText,
                        CancelReason = registration.CancelReason,
                        CreatedDate = registration.CreatedDate,
                        GuestLists = new List<GuestListDTO>
                        {
                            new GuestListDTO
                            {
                                Id = registration.Id,
                                EventGuestId = registration.Id,
                                GuestName = registration.Name,
                                GuestPhone = registration.PhoneNumber,
                                GuestEmail = registration.Email,
                                Status = guestStatus,
                                StatusText = statusText
                            }
                        },
                        MemberName = registration.Membership?.Fullname ?? registration.Name,
                        MemberPhone = registration.Membership?.PhoneNumber ?? registration.PhoneNumber,
                        MemberEmail = registration.Email,
                        MemberCompany = null,
                        MemberPosition = null
                    };
                    result.Add(guestDTO);
                }

                return result.OrderByDescending(x => x.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all event guests with registrations");
                throw;
            }
        }

        private byte ConvertRegistrationStatusToGuestStatus(int registrationStatus, string? cancelReason = null, bool isRejected = false)
        {
            // Convert EventRegistration status (0=Pending, 1=Registered, 2=CheckedIn, 3=Cancelled) 
            // to EventGuest status (0=Pending, 1=Approved, 2=Rejected, 3=Cancelled)
            // Note: EventRegistration không có status "Từ chối" riêng
            // Khi từ chối: Status = 3 (Cancelled) nhưng cần hiển thị là status 2 (Từ chối)
            if (isRejected && registrationStatus == 3)
            {
                return 2; // Từ chối
            }

            return registrationStatus switch
            {
                0 => 0, // Pending -> Pending
                1 => 1, // Registered -> Approved (đã duyệt)
                2 => 1, // CheckedIn -> Approved (đã duyệt và check-in)
                3 => 3, // Cancelled -> Cancelled (hoặc Rejected nếu isRejected = true)
                _ => 0
            };
        }

        private string GetRegistrationStatusText(int status)
        {
            // EventRegistration status: 0=Pending, 1=Registered, 2=CheckedIn, 3=Cancelled
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Đã check-in",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private EventGuestDTO MapToDTO(EventGuest eventGuest)
        {
            return new EventGuestDTO
            {
                Id = eventGuest.Id,
                EventId = eventGuest.EventId,
                EventTitle = eventGuest.Event?.Title ?? "N/A",
                UserZaloId = eventGuest.UserZaloId,
                Note = eventGuest.Note,
                GuestNumber = eventGuest.GuestNumber,
                Status = eventGuest.Status,
                StatusText = GetStatusText(eventGuest.Status),
                RejectReason = eventGuest.RejectReason,
                CancelReason = eventGuest.CancelReason,
                CreatedDate = eventGuest.CreatedDate,
                GuestLists = eventGuest.GuestLists.Select(gl => new GuestListDTO
                {
                    Id = gl.Id,
                    EventGuestId = gl.EventGuestId,
                    GuestName = gl.GuestName,
                    GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(gl.GuestPhone ?? string.Empty),
                    GuestEmail = gl.GuestEmail,
                    Status = gl.Status,
                    StatusText = GetStatusText(gl.Status),
                    CheckInCode = !string.IsNullOrEmpty(gl.CheckInCode) ? $"GUEST_{gl.CheckInCode}" : null,
                    CheckInStatus = gl.CheckInStatus
                }).ToList(),
                MemberName = eventGuest.Membership?.Fullname,
                MemberPhone = eventGuest.Membership?.PhoneNumber,
                MemberEmail = null,
                MemberCompany = null,
                MemberPosition = null
            };
        }

        private string GetStatusText(byte status)
        {
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Từ chối",
                3 => "Hủy",
                4 => "Chờ đăng ký",
                5 => "Đã đăng ký",
                _ => "Không xác định"
            };
        }

        public async Task<EventGuestDTO> ApproveGuestListItemAsync(string guestListId, ApproveGuestListRequest request)
        {
            try
            {
                var guestListItem = await _guestListRepository.AsQueryable()
                    .Where(gl => gl.Id == guestListId)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Event)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Membership)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                if (guestListItem == null)
                {
                    throw new Exception("Không tìm thấy thành viên trong danh sách");
                }

                var eventGuest = guestListItem.EventGuest;

                if (eventGuest == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký khách mời");
                }

                // Chỉ cho phép duyệt/từ chối khi đơn ở trạng thái: 0 (Chờ xử lý), 1 (Đã duyệt), hoặc 5 (Đã đăng ký)
                if (eventGuest.Status != 0 && eventGuest.Status != 1 && eventGuest.Status != 5)
                {
                    throw new Exception("Chỉ có thể duyệt/từ chối thành viên khi đơn đang ở trạng thái chờ xử lý hoặc đã duyệt hoặc đã đăng ký");
                }

                // Chỉ cho phép duyệt/từ chối thành viên khi status = 0 (Chờ xử lý) hoặc 5 (Đã đăng ký)
                if (guestListItem.Status != 0 && guestListItem.Status != 5)
                {
                    throw new Exception("Chỉ có thể duyệt/từ chối thành viên đang ở trạng thái chờ xử lý hoặc đã đăng ký");
                }

                if (request.IsApproved)
                {
                    // Kiểm tra capacity nếu sự kiện có giới hạn (chỉ khi status = 0 hoặc 5)
                    if ((guestListItem.Status == 0 || guestListItem.Status == 5) && eventGuest.Event.JoinCount > 0)
                    {
                        var registeredCount = await _eventRegistrationRepository.AsQueryable()
                            .Where(er => er.EventId == eventGuest.EventId && (er.Status == 1 || er.Status == 2))
                            .CountAsync();

                        var approvedGuestsCount = await _eventGuestRepository.AsQueryable()
                            .Where(eg => eg.EventId == eventGuest.EventId && eg.Status == 1 && eg.Id != eventGuest.Id)
                            .SumAsync(eg => eg.GuestNumber);

                        // Đếm số thành viên đã được duyệt trong đơn này (trừ thành viên đang xử lý)
                        var approvedInThisList = eventGuest.GuestLists
                            .Where(gl => gl.Id != guestListId && gl.Status == 1)
                            .Count();

                        var totalParticipants = registeredCount + approvedGuestsCount + approvedInThisList;
                        var remainingSlots = eventGuest.Event.JoinCount - totalParticipants;

                        if (remainingSlots < 1)
                        {
                            throw new Exception($"Sự kiện chỉ còn {remainingSlots} chỗ trống. Không thể duyệt thành viên này.");
                        }
                    }

                    guestListItem.Status = 1; // Đã duyệt
                    guestListItem.UpdatedDate = DateTime.Now;
                    // Chỉ tạo CheckInCode nếu chưa có (status = 0 hoặc 5)
                    if (string.IsNullOrEmpty(guestListItem.CheckInCode))
                    {
                        guestListItem.CheckInCode = GenerateRandomCode(8);
                    }
                }
                else
                {
                    guestListItem.Status = 2; // Từ chối
                }

                guestListItem.UpdatedDate = DateTime.Now;
                _guestListRepository.Update(guestListItem);

                // Kiểm tra xem tất cả thành viên đã được xử lý chưa
                var allProcessed = eventGuest.GuestLists.All(gl => gl.Status != 0);
                var allApproved = eventGuest.GuestLists.All(gl => gl.Status == 1);
                var hasRejected = eventGuest.GuestLists.Any(gl => gl.Status == 2);

                // Cập nhật status của đơn nếu tất cả thành viên đã được xử lý
                if (allProcessed)
                {
                    if (allApproved)
                    {
                        // Tất cả thành viên đã được duyệt -> đơn đã duyệt
                        eventGuest.Status = 1; // Đã duyệt
                        eventGuest.UpdatedDate = DateTime.Now;
                        _eventGuestRepository.Update(eventGuest);
                    }
                    else if (hasRejected)
                    {
                        // Có ít nhất một thành viên bị từ chối
                        // Đơn vẫn ở trạng thái đã duyệt (một phần) hoặc giữ nguyên status hiện tại
                        // Không cần thay đổi status của đơn
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Load lại với đầy đủ thông tin
                var updatedEventGuest = await _eventGuestRepository.AsQueryable()
                    .Where(eg => eg.Id == eventGuest.Id)
                    .Include(eg => eg.Event)
                    .Include(eg => eg.Membership)
                    .Include(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                return MapToDTO(updatedEventGuest!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving guest list item {GuestListId}", guestListId);
                throw;
            }
        }

        public async Task<GuestListDTO> CancelGuestListItemAsync(string guestListId, string cancelReason)
        {
            try
            {
                var guestListItem = await _guestListRepository.AsQueryable()
                    .Where(gl => gl.Id == guestListId)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Event)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Membership)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.GuestLists)
                    .FirstOrDefaultAsync();

                if (guestListItem == null)
                {
                    throw new Exception("Không tìm thấy khách mời trong danh sách");
                }

                var eventGuest = guestListItem.EventGuest;

                if (eventGuest == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký khách mời");
                }

                if (guestListItem.Status == 3)
                {
                    throw new Exception("Khách mời đã được hủy trước đó");
                }

                // Cập nhật status cho guest item
                guestListItem.Status = 3; // Hủy
                guestListItem.UpdatedDate = DateTime.Now;
                _guestListRepository.Update(guestListItem);

                // Kiểm tra xem tất cả thành viên đã bị hủy chưa
                var allCancelled = eventGuest.GuestLists.All(gl => gl.Status == 3);

                // Nếu tất cả thành viên đều bị hủy thì cập nhật status của đơn đăng ký
                if (allCancelled)
                {
                    eventGuest.Status = 3; // Hủy
                    eventGuest.CancelReason = cancelReason;
                    eventGuest.UpdatedDate = DateTime.Now;
                    _eventGuestRepository.Update(eventGuest);
                }

                var result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                    var payload = CreateEventPayloadFromData(eventGuest);
                    BackgroundJob.Schedule<EventGuestService>(
                        x => x.TriggerEventGuestEvent("Membership.UpdateEventStatus", payload),
                        TimeSpan.FromSeconds(delayInSeconds)
                    );
                }

                return new GuestListDTO
                {
                    Id = guestListItem.Id,
                    EventGuestId = guestListItem.EventGuestId,
                    GuestName = guestListItem.GuestName,
                    GuestPhone = PhoneNumberHandler.FixFormatPhoneNumber(guestListItem.GuestPhone ?? string.Empty),
                    GuestEmail = guestListItem.GuestEmail,
                    Status = guestListItem.Status,
                    StatusText = GetStatusText(guestListItem.Status),
                    CheckInCode = guestListItem.CheckInCode,
                    CheckInStatus = guestListItem.CheckInStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling guest list item {GuestListId}", guestListId);
                throw;
            }
        }

        #region Private Methods

        public async Task TriggerEventGuestEvent(string eventName, EventPayload payload)
        {
            await _mediator.Send(new EmitEventArgs
            {
                EventName = eventName,
                TriggerPhoneNumber = string.IsNullOrEmpty(payload.PhoneNumber) ? null : payload.PhoneNumber,
                TriggerZaloIdByOA = string.IsNullOrEmpty(payload.UserZaloIdByOA) ? null : payload.UserZaloIdByOA,
                Payload = payload
            });
        }

        public EventPayload CreateEventPayloadFromData(EventGuest eventGuest)
        {
            var membership = eventGuest.Membership;
            var evnt = eventGuest.Event;

            return new EventPayload
            {
                UserZaloId = membership.UserZaloId,
                UserZaloName = membership.UserZaloName,
                UserZaloIdByOA = membership.UserZaloIdByOA,
                PhoneNumber = membership.PhoneNumber,
                Note = eventGuest.Note,
                GuestNumber = eventGuest.GuestNumber,
                Status = eventGuest.Status,
                RejectReason = eventGuest.RejectReason,
                CancelReason = eventGuest.CancelReason,
                EventTitle = evnt.Title
            };
        }
        public async Task<List<EventGuestByPhoneResponse>> GetEventByPhoneAsync(string phone)
        {
            try
            {
                var fixedPhone = PhoneNumberHandler.FixFormatPhoneNumber(phone);
                var guestLists = await _guestListRepository.AsQueryable()
                    .Where(gl =>
                        gl.GuestPhone == fixedPhone  &&
                        gl.EventGuest.Status == (byte)EGuestStatus.Approved &&
                        gl.EventGuest.Event.IsActive == true &&
                        gl.EventGuest.Event.EndTime >= DateTime.Now
                    )
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Event)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Membership)
                    .ToListAsync();

                var httpContext = _httpContextAccessor.HttpContext;
                var result = new List<EventGuestByPhoneResponse>();

                foreach (var gl in guestLists)
                {
                    // Banner full URL
                    var bannerFullUrl = await _url.ToFullUrl(gl.EventGuest.Event.Banner ?? string.Empty, httpContext);

                    // Images full URL -> list<string>
                    var imagesFullUrls = new List<string>();
                    if (!string.IsNullOrEmpty(gl.EventGuest.Event.Images))
                    {
                        var paths = gl.EventGuest.Event.Images
                            .Split(',')
                            .Select(p => p.Trim())
                            .Where(p => !string.IsNullOrEmpty(p))
                            .ToList();

                        foreach (var path in paths)
                        {
                            var fullUrl = await _url.ToFullUrl(path, httpContext);
                            imagesFullUrls.Add(fullUrl);
                        }
                    }

                    result.Add(new EventGuestByPhoneResponse
                    {
                        Id = gl.EventGuest.Event.Id,
                        Title = gl.EventGuest.Event.Title,
                        GroupId = gl.EventGuest.Event.GroupId,
                        // API pending invite: chỉ cần thông tin cơ bản, GroupName để trống
                        GroupName = string.Empty,
                        Content = gl.EventGuest.Event.Content,
                        StartTime = gl.EventGuest.Event.StartTime,
                        EndTime = gl.EventGuest.Event.EndTime,
                        Type = gl.EventGuest.Event.Type,
                        Status = gl.EventGuest.Event.Status,
                        JoinCount = gl.EventGuest.Event.JoinCount,
                        IsActive = gl.EventGuest.Event.IsActive,
                        CreatedDate = gl.EventGuest.Event.CreatedDate,
                        UpdatedDate = gl.EventGuest.Event.UpdatedDate,
                        Banner = bannerFullUrl,
                        Images = imagesFullUrls,
                        MeetingLink = gl.EventGuest.Event.MeetingLink,
                        GoogleMapURL = gl.EventGuest.Event.GoogleMapURL,
                        Address = gl.EventGuest.Event.Address,
                        NeedApproval = gl.EventGuest.Event.NeedApproval,
                        // Pending lời mời: status = 0 nên IsRegister = false, IsCheckIn = false
                        IsRegister = gl.Status != (byte)EGuestStatus.Pending,
                        IsCheckIn = gl.CheckInStatus ?? false,
                        CheckInCode = !string.IsNullOrEmpty(gl.CheckInCode) ? $"GUEST_{gl.CheckInCode}" : null,
                        GuestName = gl.EventGuest.Membership?.Fullname,
                        UserZaloId = gl.EventGuest.Membership?.UserZaloId,
                        Avatar = gl.EventGuest.Membership?.ZaloAvatar,
                        GuestListId = gl.Id,
                        FormStatus = gl.Status,
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by phone {Phone}", phone);
                throw;
            }
        }


        public async Task<List<EventGuestByPhoneResponse>> GetConfirmedEventsByPhoneAsync(string phone)
        {
            try
            {
                var fixedPhone = PhoneNumberHandler.FixFormatPhoneNumber(phone);
                var guestLists = await _guestListRepository.AsQueryable()
                    .Where(gl =>
                        gl.GuestPhone == fixedPhone &&
                        gl.Status != (byte)EGuestStatus.Pending &&
                        gl.EventGuest.Status == (byte)EGuestStatus.Approved &&
                        gl.EventGuest.Event.IsActive == true &&
                        gl.EventGuest.Event.EndTime >= DateTime.Now
                    )
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Event)
                    .Include(gl => gl.EventGuest)
                        .ThenInclude(eg => eg.Membership)
                    .ToListAsync();

                var httpContext = _httpContextAccessor.HttpContext;
                var result = new List<EventGuestByPhoneResponse>();

                foreach (var gl in guestLists)
                {
                    // Lấy full chi tiết sự kiện (bao gồm gifts & sponsors)
                    var eventDetail = await _eventService.GetEventByIdAsync(gl.EventGuest.Event.Id);

                    // Banner full URL
                    var bannerFullUrl = await _url.ToFullUrl(eventDetail?.Banner ?? gl.EventGuest.Event.Banner ?? string.Empty, httpContext);

                    // Images full URL -> list<string>
                    var imagesFullUrls = new List<string>();
                    var rawImages = eventDetail?.Images ?? new List<string>();
                    if (rawImages.Any())
                    {
                        foreach (var img in rawImages)
                        {
                            if (!string.IsNullOrEmpty(img))
                            {
                                var fullUrl = await _url.ToFullUrl(img, httpContext);
                                imagesFullUrls.Add(fullUrl);
                            }
                        }
                    }
                    // Nếu eventDetail rỗng (fallback từ entity)
                    if (!imagesFullUrls.Any() && !string.IsNullOrEmpty(gl.EventGuest.Event.Images))
                    {
                        var paths = gl.EventGuest.Event.Images
                            .Split(',')
                            .Select(p => p.Trim())
                            .Where(p => !string.IsNullOrEmpty(p))
                            .ToList();

                        foreach (var path in paths)
                        {
                            var fullUrl = await _url.ToFullUrl(path, httpContext);
                            imagesFullUrls.Add(fullUrl);
                        }
                    }

                    // Gifts & sponsors từ EventDetailDTO
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

                    result.Add(new EventGuestByPhoneResponse
                    {
                        Id = eventDetail?.Id ?? gl.EventGuest.Event.Id,
                        GroupId = eventDetail?.GroupId ?? gl.EventGuest.Event.GroupId,
                        GroupName = eventDetail?.GroupName ?? string.Empty,
                        Title = eventDetail?.Title ?? gl.EventGuest.Event.Title,
                        Content = eventDetail?.Content ?? gl.EventGuest.Event.Content,
                        StartTime = eventDetail?.StartTime ?? gl.EventGuest.Event.StartTime,
                        EndTime = eventDetail?.EndTime ?? gl.EventGuest.Event.EndTime,
                        JoinCount = eventDetail?.JoinCount ?? gl.EventGuest.Event.JoinCount,
                        Type = eventDetail?.Type ?? gl.EventGuest.Event.Type,
                        TypeText = eventDetail?.TypeText ?? string.Empty,
                        Status = eventDetail?.Status ?? gl.EventGuest.Event.Status,
                        StatusText = eventDetail?.StatusText ?? string.Empty,
                        StatusClass = eventDetail?.StatusClass ?? string.Empty,
                        IsActive = eventDetail?.IsActive ?? gl.EventGuest.Event.IsActive,
                        CreatedDate = eventDetail?.CreatedDate ?? gl.EventGuest.Event.CreatedDate,
                        UpdatedDate = eventDetail?.UpdatedDate ?? gl.EventGuest.Event.UpdatedDate,
                        NeedApproval = eventDetail?.NeedApproval ?? gl.EventGuest.Event.NeedApproval,
                        Banner = bannerFullUrl,
                        Images = imagesFullUrls,
                        MeetingLink = eventDetail?.MeetingLink ?? gl.EventGuest.Event.MeetingLink,
                        GoogleMapURL = eventDetail?.GoogleMapURL ?? gl.EventGuest.Event.GoogleMapURL,
                        Address = eventDetail?.Address ?? gl.EventGuest.Event.Address,
                        // Theo yêu cầu: status khác 0 coi như đã đăng ký
                        IsRegister = gl.Status != (byte)EGuestStatus.Pending,
                        IsCheckIn = gl.CheckInStatus ?? false,
                        CheckInCode = !string.IsNullOrEmpty(gl.CheckInCode) ? $"GUEST_{gl.CheckInCode}" : null,
                        EventGifts = eventGifts,
                        EventSponsors = eventSponsors,
                        GuestName = gl.EventGuest.Membership?.Fullname,
                        UserZaloId = gl.EventGuest.Membership?.UserZaloId,
                        Avatar = gl.EventGuest.Membership?.ZaloAvatar,
                        GuestListId = gl.Id,
                        CheckInStatus = gl.CheckInStatus
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmed events by phone {Phone}", phone);
                throw;
            }
        }
        #endregion
    }
}

