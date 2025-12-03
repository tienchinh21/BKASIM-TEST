using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.DTOs.CustomFields;
using Newtonsoft.Json;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service for managing group custom fields during registration
    /// </summary>
    public class GroupCustomFieldService : IGroupCustomFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<MembershipGroup> _membershipGroupRepository;
        private readonly IRepository<CustomFieldTab> _tabRepository;
        private readonly IRepository<CustomField> _fieldRepository;
        private readonly IRepository<CustomFieldValue> _valueRepository;
        private readonly ICustomFieldValidator _validator;
        private readonly ILogger<GroupCustomFieldService> _logger;

        public GroupCustomFieldService(
            IUnitOfWork unitOfWork,
            ICustomFieldValidator validator,
            ILogger<GroupCustomFieldService> logger)
        {
            _unitOfWork = unitOfWork;
            _groupRepository = unitOfWork.GetRepository<Group>();
            _membershipGroupRepository = unitOfWork.GetRepository<MembershipGroup>();
            _tabRepository = unitOfWork.GetRepository<CustomFieldTab>();
            _fieldRepository = unitOfWork.GetRepository<CustomField>();
            _valueRepository = unitOfWork.GetRepository<CustomFieldValue>();
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<GroupCustomFieldFormDTO> GetGroupCustomFieldFormAsync(string groupId)
        {
            _logger.LogInformation("Getting custom field form for group {GroupId}", groupId);

            var group = await _groupRepository.AsQueryable()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new NotFoundException($"Group with ID {groupId} not found");
            }

            // Get all tabs for this group with EntityType = GroupMembership (exclude deleted)
            var tabs = await _tabRepository.AsQueryable()
                .Where(t => t.EntityType == ECustomFieldEntityType.GroupMembership 
                    && t.EntityId == groupId 
                    && !t.IsDelete)
                .Include(t => t.CustomFields.Where(f => !f.IsDelete))
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            var result = new GroupCustomFieldFormDTO
            {
                GroupId = groupId,
                GroupName = group.GroupName,
                Tabs = tabs.Select(t => new CustomFieldTabWithFieldsDTO
                {
                    Id = t.Id,
                    TabName = t.TabName,
                    DisplayOrder = t.DisplayOrder,
                    Fields = t.CustomFields
                        .OrderBy(f => f.DisplayOrder)
                        .Select(f => MapFieldToDTO(f))
                        .ToList()
                }).ToList()
            };

            _logger.LogInformation("Retrieved {TabCount} tabs with fields for group {GroupId}", result.Tabs.Count, groupId);
            return result;
        }

        /// <inheritdoc />
        public async Task<SubmitCustomFieldValuesResult> RegisterAndSubmitValuesAsync(
            string groupId,
            string userZaloId,
            string? reason,
            string? company,
            string? position,
            Dictionary<string, string> values)
        {
            _logger.LogInformation("Registering user {UserZaloId} to group {GroupId} with custom fields", userZaloId, groupId);

            // Validate Group exists
            var group = await _groupRepository.AsQueryable()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new NotFoundException($"Group with ID {groupId} not found");
            }

            // Check if user already registered
            var existingMembership = await _membershipGroupRepository.AsQueryable()
                .FirstOrDefaultAsync(mg => mg.GroupId == groupId && mg.UserZaloId == userZaloId);

            if (existingMembership != null)
            {
                throw new InvalidOperationException("Bạn đã đăng ký tham gia nhóm này rồi");
            }

            // Get all fields for this group
            var fields = await _fieldRepository.AsQueryable()
                .Where(f => f.EntityType == ECustomFieldEntityType.GroupMembership && f.EntityId == groupId)
                .ToListAsync();

            // Validate all values
            var errors = ValidateFieldValues(fields, values);
            if (errors.Count > 0)
            {
                return new SubmitCustomFieldValuesResult
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Step 1: Create MembershipGroup
            var membershipGroup = new MembershipGroup
            {
                UserZaloId = userZaloId,
                GroupId = groupId,
                Reason = reason,
                Company = company,
                Position = position,
                IsApproved = null, // Chờ duyệt
                HasCustomFieldsSubmitted = fields.Count > 0,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _membershipGroupRepository.AddAsync(membershipGroup);
            await _unitOfWork.SaveChangesAsync(); // Save to get membershipGroup.Id

            // Step 2: Save CustomFieldValues
            var submittedValues = new List<CustomFieldValue>();
            foreach (var field in fields)
            {
                if (values.TryGetValue(field.Id, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    var fieldValue = new CustomFieldValue
                    {
                        CustomFieldId = field.Id,
                        EntityType = ECustomFieldEntityType.GroupMembership,
                        EntityId = membershipGroup.Id,
                        FieldName = field.FieldName,
                        FieldValue = value,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    await _valueRepository.AddAsync(fieldValue);
                    submittedValues.Add(fieldValue);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully registered user {UserZaloId} to group {GroupId} with {ValueCount} custom field values",
                userZaloId, groupId, submittedValues.Count);

            return new SubmitCustomFieldValuesResult
            {
                Success = true,
                MembershipGroupId = membershipGroup.Id,
                HasCustomFieldsSubmitted = membershipGroup.HasCustomFieldsSubmitted,
                SubmittedValues = submittedValues.Select(MapValueToDTO).ToList()
            };
        }

        /// <inheritdoc />
        public async Task<SubmitCustomFieldValuesResult> SubmitValuesAsync(string membershipGroupId, Dictionary<string, string> values)
        {
            _logger.LogInformation("Submitting custom field values for membership group {MembershipGroupId}", membershipGroupId);

            // Validate MembershipGroup exists
            var membershipGroup = await _membershipGroupRepository.AsQueryable()
                .FirstOrDefaultAsync(mg => mg.Id == membershipGroupId);

            if (membershipGroup == null)
            {
                throw new NotFoundException($"Membership group with ID {membershipGroupId} not found");
            }

            // Get all fields for this group
            var fields = await _fieldRepository.AsQueryable()
                .Where(f => f.EntityType == ECustomFieldEntityType.GroupMembership && f.EntityId == membershipGroup.GroupId)
                .ToListAsync();

            // Validate all values
            var errors = ValidateFieldValues(fields, values);
            if (errors.Count > 0)
            {
                return new SubmitCustomFieldValuesResult
                {
                    Success = false,
                    MembershipGroupId = membershipGroupId,
                    Errors = errors
                };
            }

            // Save values
            var submittedValues = new List<CustomFieldValue>();
            foreach (var field in fields)
            {
                if (values.TryGetValue(field.Id, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    var fieldValue = new CustomFieldValue
                    {
                        CustomFieldId = field.Id,
                        EntityType = ECustomFieldEntityType.GroupMembership,
                        EntityId = membershipGroupId,
                        FieldName = field.FieldName,
                        FieldValue = value,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    await _valueRepository.AddAsync(fieldValue);
                    submittedValues.Add(fieldValue);
                }
            }

            // Update HasCustomFieldsSubmitted flag
            membershipGroup.HasCustomFieldsSubmitted = true;
            membershipGroup.UpdatedDate = DateTime.Now;
            _membershipGroupRepository.Update(membershipGroup);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully submitted {ValueCount} custom field values for membership group {MembershipGroupId}",
                submittedValues.Count, membershipGroupId);

            return new SubmitCustomFieldValuesResult
            {
                Success = true,
                MembershipGroupId = membershipGroupId,
                HasCustomFieldsSubmitted = true,
                SubmittedValues = submittedValues.Select(MapValueToDTO).ToList()
            };
        }

        /// <inheritdoc />
        public async Task<SubmitCustomFieldValuesResult> UpdateValuesAsync(string membershipGroupId, Dictionary<string, string> values)
        {
            _logger.LogInformation("Updating custom field values for membership group {MembershipGroupId}", membershipGroupId);

            // Validate MembershipGroup exists
            var membershipGroup = await _membershipGroupRepository.AsQueryable()
                .FirstOrDefaultAsync(mg => mg.Id == membershipGroupId);

            if (membershipGroup == null)
            {
                throw new NotFoundException($"Membership group with ID {membershipGroupId} not found");
            }

            // Get all fields for this group
            var fields = await _fieldRepository.AsQueryable()
                .Where(f => f.EntityType == ECustomFieldEntityType.GroupMembership && f.EntityId == membershipGroup.GroupId)
                .ToListAsync();

            // Validate all values
            var errors = ValidateFieldValues(fields, values);
            if (errors.Count > 0)
            {
                return new SubmitCustomFieldValuesResult
                {
                    Success = false,
                    MembershipGroupId = membershipGroupId,
                    Errors = errors
                };
            }

            // Get existing values
            var existingValues = await _valueRepository.AsQueryable()
                .Where(v => v.EntityType == ECustomFieldEntityType.GroupMembership && v.EntityId == membershipGroupId)
                .ToListAsync();

            var updatedValues = new List<CustomFieldValue>();

            foreach (var field in fields)
            {
                var existingValue = existingValues.FirstOrDefault(v => v.CustomFieldId == field.Id);
                var hasNewValue = values.TryGetValue(field.Id, out var newValue);
                var isNewValueEmpty = string.IsNullOrWhiteSpace(newValue);

                if (existingValue != null)
                {
                    if (hasNewValue && !isNewValueEmpty)
                    {
                        // Update existing value
                        existingValue.FieldValue = newValue!;
                        existingValue.UpdatedDate = DateTime.Now;
                        _valueRepository.Update(existingValue);
                        updatedValues.Add(existingValue);
                    }
                    else if (isNewValueEmpty && !field.IsRequired)
                    {
                        // Delete value for non-required field when cleared
                        _valueRepository.Delete(existingValue);
                    }
                    else if (!isNewValueEmpty)
                    {
                        updatedValues.Add(existingValue);
                    }
                }
                else if (hasNewValue && !isNewValueEmpty)
                {
                    // Create new value
                    var fieldValue = new CustomFieldValue
                    {
                        CustomFieldId = field.Id,
                        EntityType = ECustomFieldEntityType.GroupMembership,
                        EntityId = membershipGroupId,
                        FieldName = field.FieldName,
                        FieldValue = newValue!,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    await _valueRepository.AddAsync(fieldValue);
                    updatedValues.Add(fieldValue);
                }
            }

            // Update HasCustomFieldsSubmitted flag if not already set
            if (!membershipGroup.HasCustomFieldsSubmitted)
            {
                membershipGroup.HasCustomFieldsSubmitted = true;
                membershipGroup.UpdatedDate = DateTime.Now;
                _membershipGroupRepository.Update(membershipGroup);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated custom field values for membership group {MembershipGroupId}", membershipGroupId);

            return new SubmitCustomFieldValuesResult
            {
                Success = true,
                MembershipGroupId = membershipGroupId,
                HasCustomFieldsSubmitted = true,
                SubmittedValues = updatedValues.Select(MapValueToDTO).ToList()
            };
        }

        /// <inheritdoc />
        public async Task<MemberCustomFieldValuesDTO> GetMemberValuesAsync(string membershipGroupId)
        {
            _logger.LogInformation("Getting custom field values for membership group {MembershipGroupId}", membershipGroupId);

            // Validate MembershipGroup exists
            var membershipGroup = await _membershipGroupRepository.AsQueryable()
                .FirstOrDefaultAsync(mg => mg.Id == membershipGroupId);

            if (membershipGroup == null)
            {
                throw new NotFoundException($"Membership group with ID {membershipGroupId} not found");
            }

            // Get all tabs and fields for this group
            var tabs = await _tabRepository.AsQueryable()
                .Where(t => t.EntityType == ECustomFieldEntityType.GroupMembership && t.EntityId == membershipGroup.GroupId)
                .Include(t => t.CustomFields)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            // Get submitted values
            var submittedValues = await _valueRepository.AsQueryable()
                .Where(v => v.EntityType == ECustomFieldEntityType.GroupMembership && v.EntityId == membershipGroupId)
                .ToListAsync();

            var valueDict = submittedValues.ToDictionary(v => v.CustomFieldId, v => v.FieldValue);

            var result = new MemberCustomFieldValuesDTO
            {
                MembershipGroupId = membershipGroupId,
                GroupId = membershipGroup.GroupId,
                HasCustomFieldsSubmitted = membershipGroup.HasCustomFieldsSubmitted,
                Tabs = tabs.Select(t => new CustomFieldTabWithValuesDTO
                {
                    Id = t.Id,
                    TabName = t.TabName,
                    DisplayOrder = t.DisplayOrder,
                    Fields = t.CustomFields
                        .OrderBy(f => f.DisplayOrder)
                        .Select(f => new CustomFieldWithValueDTO
                        {
                            FieldId = f.Id,
                            FieldName = f.FieldName,
                            FieldType = f.FieldType,
                            FieldTypeText = f.FieldType.ToString(),
                            FieldOptions = ParseFieldOptions(f.FieldOptions),
                            IsRequired = f.IsRequired,
                            DisplayOrder = f.DisplayOrder,
                            IsProfile = f.IsProfile,
                            Value = valueDict.TryGetValue(f.Id, out var val) ? val : string.Empty
                        })
                        .ToList()
                }).ToList()
            };

            _logger.LogInformation("Retrieved custom field values for membership group {MembershipGroupId}", membershipGroupId);
            return result;
        }

        /// <inheritdoc />
        public async Task<MemberCustomFieldValuesDTO> GetProfileValuesAsync(string membershipGroupId)
        {
            _logger.LogInformation("Getting profile custom field values for membership group {MembershipGroupId}", membershipGroupId);

            var membershipGroup = await _membershipGroupRepository.AsQueryable()
                .FirstOrDefaultAsync(mg => mg.Id == membershipGroupId);

            if (membershipGroup == null)
            {
                throw new NotFoundException($"Membership group with ID {membershipGroupId} not found");
            }

            // Get tabs and fields with IsProfile = true only (exclude deleted)
            var tabs = await _tabRepository.AsQueryable()
                .Where(t => t.EntityType == ECustomFieldEntityType.GroupMembership 
                    && t.EntityId == membershipGroup.GroupId 
                    && !t.IsDelete)
                .Include(t => t.CustomFields.Where(f => !f.IsDelete && f.IsProfile))
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            var submittedValues = await _valueRepository.AsQueryable()
                .Where(v => v.EntityType == ECustomFieldEntityType.GroupMembership && v.EntityId == membershipGroupId)
                .ToListAsync();

            var valueDict = submittedValues.ToDictionary(v => v.CustomFieldId, v => v.FieldValue);

            // Filter out tabs with no profile fields
            var result = new MemberCustomFieldValuesDTO
            {
                MembershipGroupId = membershipGroupId,
                GroupId = membershipGroup.GroupId,
                HasCustomFieldsSubmitted = membershipGroup.HasCustomFieldsSubmitted,
                Tabs = tabs
                    .Where(t => t.CustomFields.Any())
                    .Select(t => new CustomFieldTabWithValuesDTO
                    {
                        Id = t.Id,
                        TabName = t.TabName,
                        DisplayOrder = t.DisplayOrder,
                        Fields = t.CustomFields
                            .OrderBy(f => f.DisplayOrder)
                            .Select(f => new CustomFieldWithValueDTO
                            {
                                FieldId = f.Id,
                                FieldName = f.FieldName,
                                FieldType = f.FieldType,
                                FieldTypeText = f.FieldType.ToString(),
                                FieldOptions = ParseFieldOptions(f.FieldOptions),
                                IsRequired = f.IsRequired,
                                DisplayOrder = f.DisplayOrder,
                                IsProfile = f.IsProfile,
                                Value = valueDict.TryGetValue(f.Id, out var val) ? val : string.Empty
                            })
                            .ToList()
                    }).ToList()
            };

            return result;
        }

        #region Private Helper Methods

        private Dictionary<string, string> ValidateFieldValues(List<CustomField> fields, Dictionary<string, string> values)
        {
            var errors = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                values.TryGetValue(field.Id, out var value);
                var validationResult = _validator.ValidateValue(field, value);

                if (!validationResult.IsValid)
                {
                    errors[field.Id] = validationResult.ErrorMessage ?? "Validation failed";
                }
            }

            return errors;
        }

        private CustomFieldDTO MapFieldToDTO(CustomField field)
        {
            return new CustomFieldDTO
            {
                Id = field.Id,
                CustomFieldTabId = field.CustomFieldTabId,
                EntityId = field.EntityId,
                FieldName = field.FieldName,
                FieldType = field.FieldType,
                FieldTypeText = field.FieldType.ToString(),
                FieldOptions = ParseFieldOptions(field.FieldOptions),
                IsRequired = field.IsRequired,
                DisplayOrder = field.DisplayOrder,
                IsProfile = field.IsProfile,
                CreatedDate = field.CreatedDate,
                UpdatedDate = field.UpdatedDate
            };
        }

        private CustomFieldValueDTO MapValueToDTO(CustomFieldValue value)
        {
            return new CustomFieldValueDTO
            {
                Id = value.Id,
                CustomFieldId = value.CustomFieldId,
                EntityId = value.EntityId,
                FieldName = value.FieldName,
                FieldValue = value.FieldValue,
                CreatedDate = value.CreatedDate,
                UpdatedDate = value.UpdatedDate
            };
        }

        private static List<string>? ParseFieldOptions(string? fieldOptionsJson)
        {
            if (string.IsNullOrWhiteSpace(fieldOptionsJson))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(fieldOptionsJson);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
