using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service for managing custom field values
    /// </summary>
    public class CustomFieldValueService : ICustomFieldValueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CustomFieldValue> _valueRepository;
        private readonly IRepository<CustomField> _fieldRepository;
        private readonly IRepository<CustomFieldTab> _tabRepository;
        private readonly ILogger<CustomFieldValueService> _logger;

        public CustomFieldValueService(
            IUnitOfWork unitOfWork,
            ILogger<CustomFieldValueService> logger)
        {
            _unitOfWork = unitOfWork;
            _valueRepository = unitOfWork.GetRepository<CustomFieldValue>();
            _fieldRepository = unitOfWork.GetRepository<CustomField>();
            _tabRepository = unitOfWork.GetRepository<CustomFieldTab>();
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all values for a specific entity, organized by tabs
        /// </summary>
        public async Task<List<CustomFieldValueDTO>> GetValuesByEntityAsync(ECustomFieldEntityType entityType, string entityId)
        {
            try
            {
                _logger.LogInformation("Getting values for entity type {EntityType}, entity ID {EntityId}", entityType, entityId);

                var values = await _valueRepository.AsQueryable()
                    .Where(v => v.EntityType == entityType && v.EntityId == entityId)
                    .OrderBy(v => v.CreatedDate)
                    .ToListAsync();

                var valueDTOs = values.Select(v => new CustomFieldValueDTO
                {
                    Id = v.Id,
                    CustomFieldId = v.CustomFieldId,
                    EntityId = v.EntityId,
                    FieldName = v.FieldName,
                    FieldValue = v.FieldValue,
                    CreatedDate = v.CreatedDate,
                    UpdatedDate = v.UpdatedDate
                }).ToList();

                _logger.LogInformation("Retrieved {ValueCount} values for entity type {EntityType}, entity ID {EntityId}",
                    valueDTOs.Count, entityType, entityId);

                return valueDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting values for entity type {EntityType}, entity ID {EntityId}", entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Stores submitted field values for an entity
        /// </summary>
        public async Task<List<CustomFieldValueDTO>> CreateValuesAsync(CreateCustomFieldValuesRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.EntityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(request.EntityId));
                }

                if (request.FieldValues == null || request.FieldValues.Count == 0)
                {
                    throw new ArgumentException("FieldValues cannot be empty", nameof(request.FieldValues));
                }

                _logger.LogInformation("Creating {ValueCount} values for entity type {EntityType}, entity ID {EntityId}",
                    request.FieldValues.Count, request.EntityType, request.EntityId);

                var createdValues = new List<CustomFieldValue>();
                var now = DateTime.Now;

                // Get all field definitions to retrieve field names
                var fieldIds = request.FieldValues.Keys.ToList();
                var fields = await _fieldRepository.AsQueryable()
                    .Where(f => fieldIds.Contains(f.Id))
                    .ToListAsync();

                var fieldMap = fields.ToDictionary(f => f.Id, f => f.FieldName);

                // Create value entities
                foreach (var kvp in request.FieldValues)
                {
                    var fieldId = kvp.Key;
                    var fieldValue = kvp.Value;

                    if (!fieldMap.TryGetValue(fieldId, out var fieldName))
                    {
                        _logger.LogWarning("Field with ID {FieldId} not found", fieldId);
                        continue;
                    }

                    var value = new CustomFieldValue
                    {
                        CustomFieldId = fieldId,
                        EntityType = request.EntityType,
                        EntityId = request.EntityId,
                        FieldName = fieldName,
                        FieldValue = fieldValue,
                        CreatedDate = now,
                        UpdatedDate = now
                    };

                    createdValues.Add(value);
                }

                if (createdValues.Count == 0)
                {
                    throw new InvalidOperationException("No valid field values to create");
                }

                await _valueRepository.AddRangeAsync(createdValues);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to save values to database");
                }

                _logger.LogInformation("Successfully created {ValueCount} values", createdValues.Count);

                var valueDTOs = createdValues.Select(v => new CustomFieldValueDTO
                {
                    Id = v.Id,
                    CustomFieldId = v.CustomFieldId,
                    EntityId = v.EntityId,
                    FieldName = v.FieldName,
                    FieldValue = v.FieldValue,
                    CreatedDate = v.CreatedDate,
                    UpdatedDate = v.UpdatedDate
                }).ToList();

                return valueDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating values with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Removes a single custom field value
        /// </summary>
        public async Task<bool> DeleteValueAsync(string valueId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(valueId))
                {
                    throw new ArgumentException("ValueId is required", nameof(valueId));
                }

                _logger.LogInformation("Deleting value with ID {ValueId}", valueId);

                var value = await _valueRepository.AsQueryable()
                    .FirstOrDefaultAsync(v => v.Id == valueId);

                if (value == null)
                {
                    _logger.LogWarning("Value with ID {ValueId} not found", valueId);
                    return false;
                }

                _valueRepository.Delete(value);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to delete value from database");
                }

                _logger.LogInformation("Successfully deleted value with ID {ValueId}", valueId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting value with ID {ValueId}", valueId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all values for a specific field
        /// </summary>
        public async Task<List<CustomFieldValueDTO>> GetValuesByFieldAsync(string fieldId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldId))
                {
                    throw new ArgumentException("FieldId is required", nameof(fieldId));
                }

                _logger.LogInformation("Getting values for field ID {FieldId}", fieldId);

                var values = await _valueRepository.AsQueryable()
                    .Where(v => v.CustomFieldId == fieldId)
                    .OrderBy(v => v.CreatedDate)
                    .ToListAsync();

                var valueDTOs = values.Select(v => new CustomFieldValueDTO
                {
                    Id = v.Id,
                    CustomFieldId = v.CustomFieldId,
                    EntityId = v.EntityId,
                    FieldName = v.FieldName,
                    FieldValue = v.FieldValue,
                    CreatedDate = v.CreatedDate,
                    UpdatedDate = v.UpdatedDate
                }).ToList();

                _logger.LogInformation("Retrieved {ValueCount} values for field ID {FieldId}",
                    valueDTOs.Count, fieldId);

                return valueDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting values for field ID {FieldId}", fieldId);
                throw;
            }
        }
    }
}
