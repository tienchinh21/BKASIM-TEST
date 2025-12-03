using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;
using Newtonsoft.Json;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service for handling form validation and submission with custom fields
    /// </summary>
    public class CustomFieldFormHandler : ICustomFieldFormHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CustomField> _fieldRepository;
        private readonly IRepository<CustomFieldValue> _valueRepository;
        private readonly ILogger<CustomFieldFormHandler> _logger;

        public CustomFieldFormHandler(
            IUnitOfWork unitOfWork,
            ILogger<CustomFieldFormHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldRepository = unitOfWork.GetRepository<CustomField>();
            _valueRepository = unitOfWork.GetRepository<CustomFieldValue>();
            _logger = logger;
        }

        /// <summary>
        /// Validates a form submission against custom field definitions
        /// </summary>
        public async Task<FormValidationResult> ValidateFormAsync(
            ECustomFieldEntityType entityType,
            string entityId,
            Dictionary<string, string> submittedValues)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(entityId));
                }

                if (submittedValues == null)
                {
                    throw new ArgumentNullException(nameof(submittedValues));
                }

                _logger.LogInformation("Validating form for entity type {EntityType}, entity ID {EntityId}",
                    entityType, entityId);

                // Get all required fields for this entity
                var requiredFields = await _fieldRepository.AsQueryable()
                    .Where(f => f.EntityType == entityType && f.EntityId == entityId && f.IsRequired)
                    .ToListAsync();

                var errors = new Dictionary<string, string>();

                // Check each required field
                foreach (var field in requiredFields)
                {
                    // Check if field ID is in submitted values
                    if (!submittedValues.ContainsKey(field.Id))
                    {
                        errors[field.Id] = $"Field '{field.FieldName}' is required";
                        _logger.LogWarning("Missing required field {FieldId} ({FieldName})", field.Id, field.FieldName);
                        continue;
                    }

                    var submittedValue = submittedValues[field.Id];

                    // Check if value is empty or whitespace
                    if (string.IsNullOrWhiteSpace(submittedValue))
                    {
                        errors[field.Id] = $"Field '{field.FieldName}' cannot be empty";
                        _logger.LogWarning("Required field {FieldId} ({FieldName}) has empty value", field.Id, field.FieldName);
                        continue;
                    }

                    // Validate field type specific rules
                    var validationError = ValidateFieldValue(field, submittedValue);
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        errors[field.Id] = validationError;
                        _logger.LogWarning("Field {FieldId} ({FieldName}) failed type validation: {Error}",
                            field.Id, field.FieldName, validationError);
                    }
                }

                // Validate optional fields that have values
                var optionalFields = await _fieldRepository.AsQueryable()
                    .Where(f => f.EntityType == entityType && f.EntityId == entityId && !f.IsRequired)
                    .ToListAsync();

                foreach (var field in optionalFields)
                {
                    if (submittedValues.ContainsKey(field.Id))
                    {
                        var submittedValue = submittedValues[field.Id];

                        // Only validate if value is provided
                        if (!string.IsNullOrWhiteSpace(submittedValue))
                        {
                            var validationError = ValidateFieldValue(field, submittedValue);
                            if (!string.IsNullOrEmpty(validationError))
                            {
                                errors[field.Id] = validationError;
                                _logger.LogWarning("Optional field {FieldId} ({FieldName}) failed type validation: {Error}",
                                    field.Id, field.FieldName, validationError);
                            }
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    _logger.LogWarning("Form validation failed with {ErrorCount} errors for entity type {EntityType}, entity ID {EntityId}",
                        errors.Count, entityType, entityId);
                    return FormValidationResult.Failure(errors);
                }

                _logger.LogInformation("Form validation passed for entity type {EntityType}, entity ID {EntityId}",
                    entityType, entityId);
                return FormValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating form for entity type {EntityType}, entity ID {EntityId}",
                    entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Validates and submits a form with custom field values
        /// </summary>
        public async Task<List<CustomFieldValueDTO>> SubmitFormAsync(
            ECustomFieldEntityType entityType,
            string entityId,
            Dictionary<string, string> submittedValues)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(entityId));
                }

                if (submittedValues == null)
                {
                    throw new ArgumentNullException(nameof(submittedValues));
                }

                _logger.LogInformation("Submitting form for entity type {EntityType}, entity ID {EntityId} with {ValueCount} values",
                    entityType, entityId, submittedValues.Count);

                // Validate the form first
                var validationResult = await ValidateFormAsync(entityType, entityId, submittedValues);

                if (!validationResult.IsValid)
                {
                    var errorMessages = string.Join("; ", validationResult.Errors.Values);
                    throw new InvalidOperationException($"Form validation failed: {errorMessages}");
                }

                // Get all fields for this entity to map field IDs to field names
                var allFields = await _fieldRepository.AsQueryable()
                    .Where(f => f.EntityType == entityType && f.EntityId == entityId)
                    .ToListAsync();

                var fieldMap = allFields.ToDictionary(f => f.Id, f => f);

                // Create CustomFieldValue entities for all submitted values
                var createdValues = new List<CustomFieldValue>();

                foreach (var (fieldId, fieldValue) in submittedValues)
                {
                    // Skip empty values for optional fields
                    if (string.IsNullOrWhiteSpace(fieldValue))
                    {
                        continue;
                    }

                    if (!fieldMap.ContainsKey(fieldId))
                    {
                        _logger.LogWarning("Field ID {FieldId} not found in entity type {EntityType}, entity ID {EntityId}",
                            fieldId, entityType, entityId);
                        continue;
                    }

                    var field = fieldMap[fieldId];

                    var customFieldValue = new CustomFieldValue
                    {
                        CustomFieldId = fieldId,
                        EntityType = entityType,
                        EntityId = entityId,
                        FieldName = field.FieldName,
                        FieldValue = fieldValue,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _valueRepository.AddAsync(customFieldValue);
                    createdValues.Add(customFieldValue);

                    _logger.LogInformation("Created custom field value for field {FieldId} ({FieldName}) with value {FieldValue}",
                        fieldId, field.FieldName, fieldValue);
                }

                // Save all values to database
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0 && createdValues.Count > 0)
                {
                    throw new InvalidOperationException("Failed to save field values to database");
                }

                var resultDTOs = createdValues.Select(MapToDTO).ToList();

                _logger.LogInformation("Successfully submitted form with {ValueCount} values for entity type {EntityType}, entity ID {EntityId}",
                    resultDTOs.Count, entityType, entityId);

                return resultDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting form for entity type {EntityType}, entity ID {EntityId}",
                    entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Validates a field value based on its field type
        /// </summary>
        private string? ValidateFieldValue(CustomField field, string value)
        {
            try
            {
                return field.FieldType switch
                {
                    EEventFieldType.Email => ValidateEmail(value),
                    EEventFieldType.PhoneNumber => ValidatePhoneNumber(value),
                    EEventFieldType.Date => ValidateDate(value),
                    EEventFieldType.DateTime => ValidateDateTime(value),
                    EEventFieldType.Integer => ValidateInteger(value),
                    EEventFieldType.Decimal => ValidateDecimal(value),
                    EEventFieldType.Url => ValidateUrl(value),
                    EEventFieldType.Dropdown => ValidateDropdownValue(field, value),
                    EEventFieldType.MultipleChoice => ValidateMultipleChoiceValue(field, value),
                    EEventFieldType.Boolean => ValidateBoolean(value),
                    EEventFieldType.YearOfBirth => ValidateYearOfBirth(value),
                    _ => null // Text, LongText, File, Image don't need validation
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating field {FieldId} ({FieldName}) with type {FieldType}",
                    field.Id, field.FieldName, field.FieldType);
                return $"Invalid value for field type {field.FieldType}";
            }
        }

        private string? ValidateEmail(string value)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(value);
                return addr.Address == value ? null : "Invalid email format";
            }
            catch
            {
                return "Invalid email format";
            }
        }

        private string? ValidatePhoneNumber(string value)
        {
            // Basic phone number validation - at least 10 digits
            var digitsOnly = System.Text.RegularExpressions.Regex.Replace(value, @"\D", "");
            return digitsOnly.Length >= 10 ? null : "Phone number must contain at least 10 digits";
        }

        private string? ValidateDate(string value)
        {
            return DateTime.TryParse(value, out _) ? null : "Invalid date format";
        }

        private string? ValidateDateTime(string value)
        {
            return DateTime.TryParse(value, out _) ? null : "Invalid date/time format";
        }

        private string? ValidateInteger(string value)
        {
            return int.TryParse(value, out _) ? null : "Invalid integer value";
        }

        private string? ValidateDecimal(string value)
        {
            return decimal.TryParse(value, out _) ? null : "Invalid decimal value";
        }

        private string? ValidateUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out _) ? null : "Invalid URL format";
        }

        private string? ValidateBoolean(string value)
        {
            var lowerValue = value.ToLower();
            return (lowerValue == "true" || lowerValue == "false" || lowerValue == "1" || lowerValue == "0")
                ? null
                : "Invalid boolean value";
        }

        private string? ValidateYearOfBirth(string value)
        {
            if (!int.TryParse(value, out var year))
            {
                return "Invalid year format";
            }

            var currentYear = DateTime.Now.Year;
            if (year < 1900 || year > currentYear)
            {
                return $"Year must be between 1900 and {currentYear}";
            }

            return null;
        }

        private string? ValidateDropdownValue(CustomField field, string value)
        {
            if (string.IsNullOrWhiteSpace(field.FieldOptions))
            {
                return null; // No options defined, accept any value
            }

            try
            {
                var options = JsonConvert.DeserializeObject<List<string>>(field.FieldOptions);
                return options?.Contains(value) == true ? null : "Selected value is not in the available options";
            }
            catch
            {
                return null; // If options can't be parsed, accept the value
            }
        }

        private string? ValidateMultipleChoiceValue(CustomField field, string value)
        {
            if (string.IsNullOrWhiteSpace(field.FieldOptions))
            {
                return null; // No options defined, accept any value
            }

            try
            {
                var options = JsonConvert.DeserializeObject<List<string>>(field.FieldOptions);
                var selectedValues = value.Split(',').Select(v => v.Trim()).ToList();

                foreach (var selectedValue in selectedValues)
                {
                    if (options?.Contains(selectedValue) != true)
                    {
                        return $"Selected value '{selectedValue}' is not in the available options";
                    }
                }

                return null;
            }
            catch
            {
                return null; // If options can't be parsed, accept the value
            }
        }

        /// <summary>
        /// Maps a CustomFieldValue entity to a CustomFieldValueDTO
        /// </summary>
        private CustomFieldValueDTO MapToDTO(CustomFieldValue value)
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
    }
}
