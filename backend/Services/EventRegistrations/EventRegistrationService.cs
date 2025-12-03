using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Helpers;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.EventRegistrations;
using MiniAppGIBA.Models.DTOs.Memberships;
using MiniAppGIBA.Models.Request.EventRegistrations;
using MiniAppGIBA.Services.Memberships;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Services.EventRegistrations
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EventRegistration> _eventRegistrationRepository;
        private readonly IMembershipService _membershipService;
        private readonly ILogger<EventRegistrationService> _logger;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<EventCustomFieldValue> _eventCustomFieldValueRepository;
        private readonly IRepository<EventCustomField> _eventCustomFieldRepository;
        private readonly IRepository<GuestList> _guestListRepository;
        public EventRegistrationService(
            IUnitOfWork unitOfWork,
            IRepository<EventRegistration> eventRegistrationRepository,
            IMembershipService membershipService,
            ILogger<EventRegistrationService> logger,
            IRepository<Event> eventRepository,
            IRepository<EventCustomFieldValue> eventCustomFieldValueRepository,
            IRepository<EventCustomField> eventCustomFieldRepository,
            IRepository<GuestList> guestListRepository)
        {
            _unitOfWork = unitOfWork;
            _eventRegistrationRepository = eventRegistrationRepository;
            _membershipService = membershipService;
            _logger = logger;
            _eventRepository = eventRepository;
            _eventCustomFieldValueRepository = eventCustomFieldValueRepository;
            _eventCustomFieldRepository = eventCustomFieldRepository;
            _guestListRepository = guestListRepository;
        }

        public async Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(string eventId, int page = 1, int pageSize = 10, string? keyword = null)
        {
            try
            {

                var query = _eventRegistrationRepository.AsQueryable()
                    .Where(er => er.EventId == eventId);


                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(er => er.Name.Contains(keyword) ||
                                            er.PhoneNumber!.Contains(keyword) ||
                                            er.Email!.Contains(keyword));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(er => er.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(er => new EventRegistrationDTO
                    {
                        Id = er.Id,
                        EventId = er.EventId,
                        UserZaloId = er.UserZaloId,
                        Name = er.Name,
                        Phone = er.PhoneNumber,
                        Email = er.Email,
                        CheckInCode = er.CheckInCode,
                        Status = er.Status,
                        StatusText = "", // Will be set after query
                        CancelReason = er.CancelReason,
                        CreatedDate = er.CreatedDate,
                        UpdatedDate = er.UpdatedDate
                    })
                    .ToListAsync();

                // Set StatusText after query execution
                foreach (var item in items)
                {
                    item.StatusText = GetStatusText(item.Status);
                }


                return new PagedResult<EventRegistrationDTO>
                {
                    Items = items,
                    TotalItems = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event registrations for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<EventRegistrationResponseDTO> CheckInAsync(string checkInCode, string eventId)
        {
            try
            {
                var type = checkInCode.Split("_")[0];
                var name = "";
                var phone = "";
                var email = "";
                if (type == "GUEST")
                {
                    var code = checkInCode.Split("_")[1];
                    Console.WriteLine(code + " " + eventId);
                    var guestList = await _guestListRepository.AsQueryable()
                        .Include(g => g.EventGuest)
                        .FirstOrDefaultAsync(g => g.CheckInCode == code && g.EventGuest.EventId == eventId);
                    if (guestList == null)
                        throw new Exception("Không tìm thấy đăng ký với mã này");
                    if (guestList.Status != 1) // Phải là đã duyệt (1) mới check-in được
                        throw new Exception("Chỉ có thể check-in khi đăng ký đã được duyệt");
                    guestList.CheckInStatus = true;
                    guestList.CheckInTime = DateTime.Now;
                    guestList.UpdatedDate = DateTime.Now;
                    name = guestList.GuestName;
                    phone = guestList.GuestPhone;
                    email = guestList.GuestEmail;
                    _guestListRepository.Update(guestList);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    var registration = await _eventRegistrationRepository.AsQueryable()
                        .FirstOrDefaultAsync(r => r.CheckInCode == checkInCode && r.EventId == eventId);

                    if (registration == null)
                        throw new Exception("Không tìm thấy đăng ký với mã này");

                    if (registration.Status == 2) // Đã check-in
                        throw new Exception("Đã check-in rồi");

                    if (registration.Status == 3) // Đã hủy
                        throw new Exception("Đăng ký đã bị hủy");

                    if (registration.Status != 1) // Phải là đã duyệt (1) mới check-in được
                        throw new Exception("Chỉ có thể check-in khi đăng ký đã được duyệt");

                    registration.Status = 2; // Checked in
                    registration.CheckInStatus = ECheckInStatus.CheckedIn;
                    registration.CheckInTime = DateTime.Now;
                    registration.UpdatedDate = DateTime.Now;
                    name = registration.Name;
                    phone = registration.PhoneNumber;
                    email = registration.Email;
                    _eventRegistrationRepository.Update(registration);
                    await _unitOfWork.SaveChangesAsync();
                }
                return new EventRegistrationResponseDTO
                {
                    Name = name,
                    Phone = phone,
                    Email = email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in registration {CheckInCode} for event {EventId}", checkInCode, eventId);
                throw;
            }
        }

        public async Task<List<EventRegistrationResponseDTO>> CheckInMultipleAsync(List<string> checkInCodes, string eventId)
        {
            try
            {
                var results = new List<EventRegistrationResponseDTO>();

                foreach (var code in checkInCodes)
                {
                    try
                    {
                        var result = await CheckInAsync(code, eventId);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to check-in code {Code} for event {EventId}", code, eventId);
                        // Continue with other codes
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk check-in for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<bool> CancelByCodeAsync(string checkInCode, string eventId)
        {
            try
            {
                var registration = await _eventRegistrationRepository.AsQueryable()
                    .FirstOrDefaultAsync(r => r.CheckInCode == checkInCode && r.EventId == eventId);

                if (registration == null)
                    return false;

                registration.Status = 3; // Cancelled
                registration.CheckInStatus = ECheckInStatus.Cancelled;
                registration.UpdatedDate = DateTime.Now;
                registration.CancelReason = "Cancelled by admin";

                _eventRegistrationRepository.Update(registration);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling registration for code {CheckInCode} and event {EventId}", checkInCode, eventId);
                throw;
            }
        }

        public async Task<byte[]> ExportParticipantsAsync(string eventId)
        {
            try
            {
                var registrations = await _eventRegistrationRepository.AsQueryable()
                    .Where(er => er.EventId == eventId)
                    .OrderBy(er => er.Name)
                    .ToListAsync();

                var data = new Dictionary<string, List<string>>();
                var names = registrations.Select(er => er.Name).ToList();
                var phones = registrations.Select(er => er.PhoneNumber ?? "").ToList();
                var emails = registrations.Select(er => er.Email ?? "").ToList();
                var checkInCodes = registrations.Select(er => er.CheckInCode ?? "").ToList();
                var statuses = registrations.Select(er => GetStatusText(er.Status)).ToList();
                var createdDates = registrations.Select(er => er.CreatedDate.ToString("dd/MM/yyyy HH:mm")).ToList();

                data["Tên"] = names;
                data["Số điện thoại"] = phones;
                data["Email"] = emails;
                data["Mã check-in"] = checkInCodes;
                data["Trạng thái"] = statuses;
                data["Ngày đăng ký"] = createdDates;

                return await ExportHandler.ExportData("Danh sách đăng ký sự kiện", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting participants for event {EventId}", eventId);
                throw;
            }
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Đã check-in",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetCheckInStatusText(int status)
        {
            return status switch
            {
                1 => "Chưa check-in",
                2 => "Đã check-in",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        // Mini app APIs
        public async Task<EventRegistrationDTO> RegisterEventAsync(string eventId, string userZaloId, RegisterEventRequest request)
        {
            try
            {
                // 1. Kiểm tra user đã đăng ký sự kiện này chưa
                var existingRegistration = await _eventRegistrationRepository.AsQueryable()
                    .FirstOrDefaultAsync(er => er.EventId == eventId && er.UserZaloId == userZaloId);

                if (existingRegistration != null)
                {
                    if (existingRegistration.Status == 3) // Đã hủy, cho phép đăng ký lại
                    {
                        existingRegistration.Status = 0; // Pending
                        existingRegistration.Name = request.Name;
                        existingRegistration.PhoneNumber = request.PhoneNumber;
                        existingRegistration.Email = request.Email;
                        existingRegistration.UpdatedDate = DateTime.Now;

                        _eventRegistrationRepository.Update(existingRegistration);
                        await _unitOfWork.SaveChangesAsync();

                        return MapToEventRegistrationDTO(existingRegistration);
                    }
                    else
                    {
                        throw new Exception("Bạn đã đăng ký sự kiện này rồi.");
                    }
                }
                var eventEntity = await _eventRepository.AsQueryable()
                    .FirstOrDefaultAsync(e => e.Id == eventId);
                if (eventEntity == null)
                {
                    throw new Exception("Sự kiện không tồn tại.");
                }
                // 2. Tạo mã check-in unique
                string? checkInCode;
                int status;
                if (eventEntity.NeedApproval)
                {
                    checkInCode = null;
                    status = (int)ERegistrationStatus.Pending;
                }
                else
                {
                    do
                    {
                        checkInCode = GenerateRandomCode(8);
                    } while (await _eventRegistrationRepository.AsQueryable()
                        .AnyAsync(er => er.CheckInCode == checkInCode));
                    status = (int)ERegistrationStatus.Registered;
                }

                // 3. Tạo đăng ký mới
                var registration = new EventRegistration
                {
                    EventId = eventId,
                    UserZaloId = userZaloId,
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    CheckInCode = checkInCode,
                    Status = status, // Pending or Approved
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                if (request.CustomFields != null && request.CustomFields.Any())
                {
                    // Lấy danh sách EventCustomField để lấy FieldName
                    var customFieldIds = request.CustomFields.Select(cf => cf.EventCustomFieldId).ToList();
                    var eventCustomFields = await _eventCustomFieldRepository.AsQueryable()
                        .Where(cf => customFieldIds.Contains(cf.Id))
                        .ToDictionaryAsync(cf => cf.Id, cf => cf.FieldName);

                    foreach (var customField in request.CustomFields)
                    {
                        var fieldName = eventCustomFields.TryGetValue(customField.EventCustomFieldId, out var name) ? name : "";
                        var customFieldValue = new EventCustomFieldValue
                        {
                            EventRegistrationId = registration.Id,
                            EventCustomFieldId = customField.EventCustomFieldId,
                            FieldName = fieldName, // Lưu tên field để hiển thị khi field bị xóa
                            FieldValue = customField.FieldValue,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };
                        _eventCustomFieldValueRepository.Add(customFieldValue);
                    }
                }
                _eventRegistrationRepository.Add(registration);
                await _unitOfWork.SaveChangesAsync();

                return MapToEventRegistrationDTO(registration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering event {EventId} for user {UserZaloId}", eventId, userZaloId);
                throw;
            }
        }

        public async Task<List<EventRegistrationDTO>> GetUserEventRegistrationsAsync(string userZaloId)
        {
            try
            {
                var registrations = await _eventRegistrationRepository.AsQueryable()
                    .Where(er => er.UserZaloId == userZaloId && er.Status != 3) // Không lấy những đăng ký đã hủy (status 3)
                    .Include(er => er.Event)
                    .OrderByDescending(er => er.CreatedDate)
                    .ToListAsync();

                return registrations.Select(MapToEventRegistrationDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event registrations for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<bool> CancelEventRegistrationAsync(string eventId, string userZaloId)
        {
            try
            {
                var registration = await _eventRegistrationRepository.AsQueryable()
                    .FirstOrDefaultAsync(er => er.EventId == eventId && er.UserZaloId == userZaloId);

                if (registration == null || registration.Status == 3)
                    return false;

                if (registration.Status == 2) // Đã check-in (status 2)
                    throw new Exception("Không thể hủy đăng ký sau khi đã check-in.");

                registration.Status = 3; // Cancelled
                registration.UpdatedDate = DateTime.Now;
                registration.CancelReason = "Cancelled by user";

                _eventRegistrationRepository.Update(registration);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling event registration for event {EventId} and user {UserZaloId}", eventId, userZaloId);
                throw;
            }
        }

        public async Task<PagedResult<object>> GetAllUserRegistrationsAsync(
            string userZaloId,
            int page = 1,
            int pageSize = 10,
            int? type = null,
            string? keyword = null,
            byte? status = null)
        {
            try
            {
                var allRegistrations = new List<object>();

                // Type 1: Lấy đăng ký đơn lẻ (EventRegistration)
                if (type == null || type == 1)
                {
                    var registrationQuery = _eventRegistrationRepository.AsQueryable()
                        .Where(er => er.UserZaloId == userZaloId && er.Status != 3); // Không lấy đã hủy

                    // Filter by keyword
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        registrationQuery = registrationQuery.Where(er =>
                            er.Event.Title.Contains(keyword) ||
                            er.Name.Contains(keyword));
                    }

                    // Filter by status
                    if (status.HasValue)
                    {
                        registrationQuery = registrationQuery.Where(er => er.Status == status.Value);
                    }

                    var registrations = await registrationQuery
                        .Include(er => er.Event)
                            .ThenInclude(e => e.Group)
                        .Include(er => er.Membership)
                        .OrderByDescending(er => er.CreatedDate)
                        .ToListAsync();

                    foreach (var reg in registrations)
                    {
                        allRegistrations.Add(new
                        {
                            id = reg.Id,
                            type = 1, // Đăng ký đơn lẻ
                            typeText = "Đăng ký đơn lẻ",
                            eventId = reg.EventId,
                            eventTitle = reg.Event?.Title ?? "N/A",
                            eventStartTime = reg.Event?.StartTime,
                            eventEndTime = reg.Event?.EndTime,
                            eventAddress = reg.Event?.Address,
                            groupId = reg.Event?.GroupId,
                            groupName = reg.Event?.Group?.GroupName ?? "N/A",
                            name = reg.Name,
                            phoneNumber = reg.PhoneNumber,
                            email = reg.Email,
                            checkInCode = reg.CheckInCode,
                            status = reg.Status,
                            statusText = GetRegistrationStatusText(reg.Status),
                            checkInStatus = reg.CheckInStatus,
                            cancelReason = reg.CancelReason,
                            createdDate = reg.CreatedDate,
                            updatedDate = reg.UpdatedDate
                        });
                    }
                }

                // Type 2: Lấy đăng ký khách mời nhóm (EventGuest)
                if (type == null || type == 2)
                {
                    var eventGuestRepository = _unitOfWork.GetRepository<EventGuest>();
                    var guestQuery = eventGuestRepository.AsQueryable()
                        .Where(eg => eg.UserZaloId == userZaloId);

                    // Filter by keyword
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        guestQuery = guestQuery.Where(eg =>
                            eg.Event.Title.Contains(keyword) ||
                            eg.Note.Contains(keyword));
                    }

                    // Filter by status
                    if (status.HasValue)
                    {
                        // if (status == 2)
                        // {
                        //     guestQuery = guestQuery.Where(eg => eg.GuestLists.Any(s => s.CheckInStatus == true));
                        // }
                        // else
                        // {
                            guestQuery = guestQuery.Where(eg => eg.Status == status.Value);
                        // }

                    }

                    var eventGuests = await guestQuery
                        .Include(eg => eg.Event)
                            .ThenInclude(e => e.Group)
                        .Include(eg => eg.Membership)
                        .Include(eg => eg.GuestLists)
                        .OrderByDescending(eg => eg.CreatedDate)
                        .ToListAsync();

                    foreach (var guest in eventGuests)
                    {
                        allRegistrations.Add(new
                        {
                            id = guest.Id,
                            type = 2, // Đăng ký khách mời nhóm
                            typeText = "Đăng ký khách mời",
                            eventId = guest.EventId,
                            eventTitle = guest.Event?.Title ?? "N/A",
                            eventStartTime = guest.Event?.StartTime,
                            eventEndTime = guest.Event?.EndTime,
                            eventAddress = guest.Event?.Address,
                            groupId = guest.Event?.GroupId,
                            groupName = guest.Event?.Group?.GroupName ?? "N/A",
                            note = guest.Note,
                            guestNumber = guest.GuestNumber,
                            guestLists = guest.GuestLists?.Select(gl => new
                            {
                                id = gl.Id,
                                guestName = gl.GuestName,
                                guestPhone = gl.GuestPhone,
                                guestEmail = gl.GuestEmail,
                                status = gl.Status,
                                checkInStatus = gl.CheckInStatus,
                                checkInCode = !string.IsNullOrEmpty(gl.CheckInCode) ? $"GUEST_{gl.CheckInCode}" : null
                            }).ToList(),
                            status = guest.Status,
                            statusText = GetGuestStatusText(guest.Status),
                            rejectReason = guest.RejectReason,
                            cancelReason = guest.CancelReason,
                            createdDate = guest.CreatedDate,
                            updatedDate = guest.UpdatedDate
                        });
                    }
                }

                // Sort by CreatedDate DESC
                var sortedRegistrations = allRegistrations
                    .OrderByDescending(r => ((dynamic)r).createdDate)
                    .ToList();

                // Pagination
                var totalCount = sortedRegistrations.Count;
                var pagedItems = sortedRegistrations
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResult<object>
                {
                    Items = pagedItems,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all registrations for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        private string GetRegistrationStatusText(int status)
        {
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Đã check-in",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetGuestStatusText(byte status)
        {
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Từ chối",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public async Task<object> GetEventDetailForUserAsync(string eventId, string userZaloId)
        {
            return new { message = "API đang được phát triển" };
        }

        private EventRegistrationDTO MapToEventRegistrationDTO(EventRegistration registration)
        {
            return new EventRegistrationDTO
            {
                Id = registration.Id,
                EventId = registration.EventId,
                EventTitle = registration.Event?.Title ?? "N/A",
                UserZaloId = registration.UserZaloId,
                Name = registration.Name,
                PhoneNumber = registration.PhoneNumber,
                Email = registration.Email,
                CheckInCode = registration.CheckInCode,
                Status = registration.Status,
                StatusText = GetStatusText(registration.Status),
                StatusClass = GetStatusClass(registration.Status),
                CancelReason = registration.CancelReason,
                CreatedDate = registration.CreatedDate,
                UpdatedDate = registration.UpdatedDate
            };
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GetStatusClass(int status)
        {
            return status switch
            {
                1 => "warning",  // pending
                2 => "success",  // confirmed
                3 => "danger",   // cancelled
                _ => "secondary"
            };
        }
    }
}