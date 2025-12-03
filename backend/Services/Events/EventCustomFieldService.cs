using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Base.Helper;
namespace MiniAppGIBA.Services.Events
{
    public class EventCustomFieldService : IEventCustomFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EventCustomField> _eventCustomFieldRepository;
        private readonly IRepository<EventCustomFieldValue> _eventCustomFieldValueRepository;
        private readonly ILogger<EventCustomFieldService> _logger;
        private readonly IRepository<GuestList> _guestListRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<EventGuest> _eventGuestRepository;
        public EventCustomFieldService(
            IUnitOfWork unitOfWork,
            ILogger<EventCustomFieldService> logger)
        {
            _unitOfWork = unitOfWork;
            _eventCustomFieldRepository = unitOfWork.GetRepository<EventCustomField>();
            _eventCustomFieldValueRepository = unitOfWork.GetRepository<EventCustomFieldValue>();
            _logger = logger;
            _guestListRepository = unitOfWork.GetRepository<GuestList>();
            _eventRepository = unitOfWork.GetRepository<Event>();
            _eventGuestRepository = unitOfWork.GetRepository<EventGuest>();
        }

        public async Task<List<EventCustomFieldDTO>> GetEventCustomFieldsAsync(string eventId)
        {
            try
            {
                var fields = await _eventCustomFieldRepository.AsQueryable()
                    .Where(f => f.EventId == eventId)
                    .OrderBy(f => f.CreatedDate)
                    .ToListAsync();

                return fields.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event custom fields for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<EventCustomFieldDTO> CreateEventCustomFieldAsync(CreateEventCustomFieldRequest request)
        {
            try
            {
                var entity = new EventCustomField
                {
                    EventId = request.EventId,
                    FieldName = request.FieldName,
                    FieldValue = request.FieldValue,
                    FieldType = request.FieldType,
                    IsRequired = request.IsRequired,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _eventCustomFieldRepository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                // Reload to get generated ID
                var createdField = await _eventCustomFieldRepository.FindByIdAsync(entity.Id);
                return MapToDTO(createdField!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event custom field with request: {@Request}", request);
                throw;
            }
        }

        public async Task<EventCustomFieldDTO> UpdateEventCustomFieldAsync(UpdateEventCustomFieldRequest request)
        {
            try
            {
                var entity = await _eventCustomFieldRepository.FindByIdAsync(request.Id);
                if (entity == null)
                {
                    throw new NotFoundException("Không tìm thấy trường tùy chỉnh");
                }

                entity.EventId = request.EventId;
                entity.FieldName = request.FieldName;
                entity.FieldValue = request.FieldValue;
                entity.FieldType = request.FieldType;
                entity.IsRequired = request.IsRequired;
                entity.UpdatedDate = DateTime.Now;

                _eventCustomFieldRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                // Reload to get updated data
                var updatedField = await _eventCustomFieldRepository.FindByIdAsync(entity.Id);
                return MapToDTO(updatedField!);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event custom field with request: {@Request}", request);
                throw;
            }
        }

        public async Task<bool> DeleteEventCustomFieldAsync(string id)
        {
            try
            {
                var entity = await _eventCustomFieldRepository.FindByIdAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _eventCustomFieldRepository.Delete(entity);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event custom field {Id}", id);
                throw;
            }
        }

        private EventCustomFieldDTO MapToDTO(EventCustomField entity)
        {
            return new EventCustomFieldDTO
            {
                Id = entity.Id,
                EventId = entity.EventId,
                FieldName = entity.FieldName,
                FieldValue = entity.FieldValue,
                FieldType = entity.FieldType,
                FieldTypeText = GetFieldTypeText(entity.FieldType),
                IsRequired = entity.IsRequired,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        private string? GetFieldTypeText(EEventFieldType? fieldType)
        {
            if (!fieldType.HasValue)
                return null;

            return fieldType.Value switch
            {
                EEventFieldType.Text => "Văn bản",
                EEventFieldType.Integer => "Số nguyên",
                EEventFieldType.Decimal => "Số thập phân",
                EEventFieldType.YearOfBirth => "Năm sinh",
                EEventFieldType.Boolean => "Boolean (Đúng/Sai)",
                EEventFieldType.DateTime => "Ngày giờ",
                EEventFieldType.Date => "Ngày",
                EEventFieldType.Email => "Email",
                EEventFieldType.PhoneNumber => "Số điện thoại",
                EEventFieldType.Url => "Đường dẫn URL",
                EEventFieldType.LongText => "Văn bản dài",
                EEventFieldType.Dropdown => "Danh sách lựa chọn",
                EEventFieldType.MultipleChoice => "Đa lựa chọn",
                EEventFieldType.File => "File đính kèm",
                EEventFieldType.Image => "Hình ảnh",
                _ => null
            };
        }

        public async Task<List<EventCustomFieldValueDTO>> CreateEventCustomFieldValuesAsync(string GuestListId, List<CreateEventCustomFieldValueRequest> requests)
        {
            try
            {
                var guestList = await _guestListRepository.FindByIdAsync(GuestListId);
                if (guestList == null)
                {
                    throw new ArgumentException("Không tìm thấy khách mời");
                }
                if (requests == null || !requests.Any())
                {
                    var eventGuest = await _eventGuestRepository.AsQueryable()
                .Where(eg => eg.GuestLists.Any(gl => gl.Id == GuestListId))
                .Include(eg => eg.Event)
                .FirstOrDefaultAsync();
                    if (eventGuest?.Event != null)
                    {
                        if (eventGuest.Event.NeedApproval)
                        {
                            guestList.Status = (byte)EGuestStatus.Registered;
                        }
                        else
                        {
                            guestList.Status = (byte)EGuestStatus.Approved;
                            guestList.CheckInCode = GenerateMembershipCode.GenerateCode(8);
                            while (await _guestListRepository.AsQueryable()
                                .AnyAsync(gl => gl.CheckInCode == guestList.CheckInCode))
                            {
                                guestList.CheckInCode = GenerateMembershipCode.GenerateCode(8);
                            }
                        }
                    }
                    else
                    {
                        // Fallback: nếu không tìm thấy event, set Registered
                        guestList.Status = (byte)EGuestStatus.Registered;
                    }

                    guestList.UpdatedDate = DateTime.Now;
                    _guestListRepository.Update(guestList);
                    await _unitOfWork.SaveChangesAsync();
                    return new List<EventCustomFieldValueDTO>();
                }


                // Lấy tất cả EventCustomFieldIds từ requests để kiểm tra IsRequired
                var customFieldIds = requests.Select(r => r.EventCustomFieldId).Distinct().ToList();
                var customFields = await _eventCustomFieldRepository.AsQueryable()
                    .Where(f => customFieldIds.Contains(f.Id))
                    .ToListAsync();

                var customFieldsDict = customFields.ToDictionary(f => f.Id, f => f);

                // Kiểm tra các field IsRequired = true phải có trong request
                var requiredFieldIds = customFields.Where(f => f.IsRequired).Select(f => f.Id).ToList();
                var providedFieldIds = requests.Select(r => r.EventCustomFieldId).ToList();
                var missingRequiredFields = requiredFieldIds.Except(providedFieldIds).ToList();

                if (missingRequiredFields.Any())
                {
                    var missingFieldNames = customFields
                        .Where(f => missingRequiredFields.Contains(f.Id))
                        .Select(f => f.FieldName)
                        .ToList();
                    throw new ArgumentException($"Các trường bắt buộc sau chưa được điền: {string.Join(", ", missingFieldNames)}");
                }

                var entities = new List<EventCustomFieldValue>();
                var now = DateTime.Now;

                foreach (var request in requests)
                {
                    // Validate: Kiểm tra EventCustomField có tồn tại không
                    if (!customFieldsDict.TryGetValue(request.EventCustomFieldId, out var customField))
                    {
                        throw new ArgumentException($"Không tìm thấy custom field với ID: {request.EventCustomFieldId}");
                    }

                    // Validate: Nếu IsRequired = true thì FieldValue không được rỗng
                    if (customField.IsRequired && string.IsNullOrWhiteSpace(request.FieldValue))
                    {
                        throw new ArgumentException($"Trường '{customField.FieldName}' là bắt buộc và không được để trống");
                    }

                    // Nếu IsRequired = false và FieldValue rỗng, có thể bỏ qua (không lưu)
                    if (!customField.IsRequired && string.IsNullOrWhiteSpace(request.FieldValue))
                    {
                        continue; // Bỏ qua field không bắt buộc và không có giá trị
                    }

                    var entity = new EventCustomFieldValue
                    {
                        EventCustomFieldId = request.EventCustomFieldId,
                        EventRegistrationId = null, // Đăng ký đơn lẻ sẽ xử lý trong API đăng ký sự kiện
                        GuestListId = GuestListId, // Chỉ dùng cho đăng ký theo nhóm
                        FieldName = customField.FieldName, // Lưu tên field để hiển thị khi field bị xóa
                        FieldValue = request.FieldValue ?? string.Empty,
                        CreatedDate = now,
                        UpdatedDate = now
                    };

                    entities.Add(entity);
                }

                // Add all entities
                foreach (var entity in entities)
                {
                    await _eventCustomFieldValueRepository.AddAsync(entity);
                }
                var eventt = await _eventRepository.FindByIdAsync(customFields.FirstOrDefault()?.EventId);

                if (eventt.NeedApproval)
                {
                    guestList.Status = (byte)EGuestStatus.Registered;
                }
                else
                {
                    guestList.Status = (byte)EGuestStatus.Approved;
                    guestList.CheckInCode = GenerateMembershipCode.GenerateCode(8);
                    while (await _guestListRepository.AsQueryable()
                        .AnyAsync(gl => gl.CheckInCode == guestList.CheckInCode))
                    {
                        guestList.CheckInCode = GenerateMembershipCode.GenerateCode(8);
                    }
                }

                guestList.UpdatedDate = DateTime.Now;
                _guestListRepository.Update(guestList);

                await _unitOfWork.SaveChangesAsync();

                // Map to DTOs
                return entities.Select(MapToValueDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event custom field values with requests: {@Requests}", requests);
                throw;
            }
        }

        private EventCustomFieldValueDTO MapToValueDTO(EventCustomFieldValue entity)
        {
            return new EventCustomFieldValueDTO
            {
                Id = entity.Id,
                EventCustomFieldId = entity.EventCustomFieldId,
                EventRegistrationId = entity.EventRegistrationId,
                GuestListId = entity.GuestListId,
                FieldValue = entity.FieldValue,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }
        public async Task<List<EventCustomFieldValueDTO>> GetListValueCustomFieldsAsync(string? GuestListId = null, string? EventRegistrationId = null)
        {
            try
            {
                var values = _eventCustomFieldValueRepository.AsQueryable();

                if (GuestListId != null)
                {
                    values = values.Where(v => v.GuestListId == GuestListId);
                }
                if (EventRegistrationId != null)
                {
                    values = values.Where(v => v.EventRegistrationId == EventRegistrationId);
                }

                var valueList = await values.ToListAsync();

                // Lấy danh sách EventCustomFieldId để join
                var customFieldIds = valueList.Select(v => v.EventCustomFieldId).Distinct().ToList();
                var customFields = await _eventCustomFieldRepository.AsQueryable()
                    .Where(cf => customFieldIds.Contains(cf.Id))
                    .ToListAsync();

                var customFieldsDict = customFields.ToDictionary(cf => cf.Id, cf => cf);

                // Map to DTO với fieldName
                return valueList.Select(v =>
                {
                    var dto = MapToValueDTO(v);
                    if (customFieldsDict.TryGetValue(v.EventCustomFieldId, out var customField))
                    {
                        dto.FieldName = customField.FieldName;
                    }
                    return dto;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list value custom fields with GuestListId: {GuestListId} and EventRegistrationId: {EventRegistrationId}", GuestListId, EventRegistrationId);
                throw;
            }
        }
    }
}