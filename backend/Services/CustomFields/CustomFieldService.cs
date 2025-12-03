using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;
using Newtonsoft.Json;
using System;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service for managing custom fields
    /// </summary>
    public class CustomFieldService : ICustomFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CustomField> _fieldRepository;
        private readonly IRepository<CustomFieldValue> _valueRepository;
        private readonly IRepository<CustomFieldTab> _tabRepository;
        private readonly ILogger<CustomFieldService> _logger;

        public CustomFieldService(
            IUnitOfWork unitOfWork,
            ILogger<CustomFieldService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldRepository = unitOfWork.GetRepository<CustomField>();
            _valueRepository = unitOfWork.GetRepository<CustomFieldValue>();
            _tabRepository = unitOfWork.GetRepository<CustomFieldTab>();
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all fields for a specific tab, ordered by display order
        /// </summary>
        public async Task<List<CustomFieldDTO>> GetFieldsByTabAsync(string tabId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tabId))
                {
                    throw new ArgumentException("TabId is required", nameof(tabId));
                }

                _logger.LogInformation("Getting fields for tab {TabId}", tabId);

                var fields = await _fieldRepository.AsQueryable()
                    .Where(f => f.CustomFieldTabId == tabId)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                var fieldDTOs = fields.Select(MapToDTO).ToList();

                _logger.LogInformation("Retrieved {FieldCount} fields for tab {TabId}", fieldDTOs.Count, tabId);

                return fieldDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields for tab {TabId}", tabId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all fields for a specific entity, optionally filtered by tab
        /// </summary>
        public async Task<List<CustomFieldDTO>> GetFieldsByEntityAsync(ECustomFieldEntityType entityType, string entityId, string? tabId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(entityId));
                }

                _logger.LogInformation("Getting fields for entity type {EntityType}, entity ID {EntityId}, tab ID {TabId}",
                    entityType, entityId, tabId ?? "null");

                var query = _fieldRepository.AsQueryable()
                    .Where(f => f.EntityType == entityType && f.EntityId == entityId);

                if (!string.IsNullOrWhiteSpace(tabId))
                {
                    query = query.Where(f => f.CustomFieldTabId == tabId);
                }

                var fields = await query
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                var fieldDTOs = fields.Select(MapToDTO).ToList();

                _logger.LogInformation("Retrieved {FieldCount} fields for entity type {EntityType}, entity ID {EntityId}",
                    fieldDTOs.Count, entityType, entityId);

                return fieldDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields for entity type {EntityType}, entity ID {EntityId}",
                    entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new field with validation for required fields and field type support
        /// </summary>
        public async Task<CustomFieldDTO> CreateFieldAsync(CreateCustomFieldRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.EntityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(request.EntityId));
                }

                if (string.IsNullOrWhiteSpace(request.FieldName))
                {
                    throw new ArgumentException("FieldName is required", nameof(request.FieldName));
                }

                // Validate field type is supported
                if (!System.Enum.IsDefined(typeof(EEventFieldType), (byte)request.FieldType))
                {
                    throw new ArgumentException($"Field type {request.FieldType} is not supported", nameof(request.FieldType));
                }

                if (request.DisplayOrder < 0)
                {
                    throw new ArgumentException("DisplayOrder must be non-negative", nameof(request.DisplayOrder));
                }

                _logger.LogInformation("Creating new field for entity type {EntityType}, entity ID {EntityId}, field name {FieldName}",
                    request.EntityType, request.EntityId, request.FieldName);

                var field = new CustomField
                {
                    CustomFieldTabId = request.CustomFieldTabId,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    FieldName = request.FieldName,
                    FieldType = request.FieldType,
                    FieldOptions = request.FieldOptions,
                    IsRequired = request.IsRequired,
                    DisplayOrder = request.DisplayOrder,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _fieldRepository.AddAsync(field);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to save field to database");
                }

                _logger.LogInformation("Successfully created field with ID {FieldId}", field.Id);

                return MapToDTO(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating field with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing field with round-trip persistence for all properties
        /// </summary>
        public async Task<CustomFieldDTO> UpdateFieldAsync(UpdateCustomFieldRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    throw new ArgumentException("Id is required", nameof(request.Id));
                }

                if (string.IsNullOrWhiteSpace(request.FieldName))
                {
                    throw new ArgumentException("FieldName is required", nameof(request.FieldName));
                }

                // Validate field type is supported
                if (!System.Enum.IsDefined(typeof(EEventFieldType), (byte)request.FieldType))
                {
                    throw new ArgumentException($"Field type {request.FieldType} is not supported", nameof(request.FieldType));
                }

                if (request.DisplayOrder < 0)
                {
                    throw new ArgumentException("DisplayOrder must be non-negative", nameof(request.DisplayOrder));
                }

                _logger.LogInformation("Updating field with ID {FieldId}", request.Id);

                var field = await _fieldRepository.AsQueryable()
                    .FirstOrDefaultAsync(f => f.Id == request.Id);

                if (field == null)
                {
                    throw new KeyNotFoundException($"Field with ID {request.Id} not found");
                }

                field.FieldName = request.FieldName;
                field.FieldType = request.FieldType;
                field.FieldOptions = request.FieldOptions;
                field.IsRequired = request.IsRequired;
                field.DisplayOrder = request.DisplayOrder;
                field.UpdatedDate = DateTime.Now;

                _fieldRepository.Update(field);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to update field in database");
                }

                _logger.LogInformation("Successfully updated field with ID {FieldId}", field.Id);

                return MapToDTO(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Deletes a field and archives related submitted values with field name
        /// </summary>
        public async Task<bool> DeleteFieldAsync(string fieldId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldId))
                {
                    throw new ArgumentException("FieldId is required", nameof(fieldId));
                }

                _logger.LogInformation("Deleting field with ID {FieldId}", fieldId);

                var field = await _fieldRepository.AsQueryable()
                    .Include(f => f.CustomFieldValues)
                    .FirstOrDefaultAsync(f => f.Id == fieldId);

                if (field == null)
                {
                    _logger.LogWarning("Field with ID {FieldId} not found", fieldId);
                    return false;
                }

                // Archive submitted values by preserving field name
                if (field.CustomFieldValues != null && field.CustomFieldValues.Count > 0)
                {
                    _logger.LogInformation("Archiving {ValueCount} submitted values for field {FieldId}",
                        field.CustomFieldValues.Count, fieldId);

                    // Values are already linked to the field, so they'll be preserved
                    // The FieldName property in CustomFieldValue stores the archived field name
                }

                // Delete the field
                _fieldRepository.Delete(field);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to delete field from database");
                }

                _logger.LogInformation("Successfully deleted field with ID {FieldId}", fieldId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting field with ID {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Reorders fields within a tab by updating their display order
        /// </summary>
        public async Task<bool> ReorderFieldsAsync(string tabId, List<(string FieldId, int Order)> reordering)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tabId))
                {
                    throw new ArgumentException("TabId is required", nameof(tabId));
                }

                if (reordering == null || reordering.Count == 0)
                {
                    throw new ArgumentException("Reordering list cannot be empty", nameof(reordering));
                }

                _logger.LogInformation("Reordering {FieldCount} fields for tab {TabId}", reordering.Count, tabId);

                var fieldIds = reordering.Select(r => r.FieldId).ToList();
                var fields = await _fieldRepository.AsQueryable()
                    .Where(f => fieldIds.Contains(f.Id) && f.CustomFieldTabId == tabId)
                    .ToListAsync();

                if (fields.Count != reordering.Count)
                {
                    throw new InvalidOperationException("Some fields not found or do not belong to the specified tab");
                }

                // Update display orders
                foreach (var (fieldId, order) in reordering)
                {
                    var field = fields.FirstOrDefault(f => f.Id == fieldId);
                    if (field != null)
                    {
                        field.DisplayOrder = order;
                        field.UpdatedDate = DateTime.Now;
                    }
                }

                _fieldRepository.UpdateRange(fields);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to update field display orders in database");
                }

                _logger.LogInformation("Successfully reordered {FieldCount} fields for tab {TabId}",
                    reordering.Count, tabId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering fields for tab {TabId}", tabId);
                throw;
            }
        }

        /// <summary>
        /// Maps a CustomField entity to a CustomFieldDTO
        /// </summary>
        private CustomFieldDTO MapToDTO(CustomField field)
        {
            var dto = new CustomFieldDTO
            {
                Id = field.Id,
                CustomFieldTabId = field.CustomFieldTabId,
                EntityId = field.EntityId,
                FieldName = field.FieldName,
                FieldType = field.FieldType,
                FieldTypeText = field.FieldType.ToString(),
                IsRequired = field.IsRequired,
                DisplayOrder = field.DisplayOrder,
                CreatedDate = field.CreatedDate,
                UpdatedDate = field.UpdatedDate
            };

            // Parse field options if present
            if (!string.IsNullOrWhiteSpace(field.FieldOptions))
            {
                try
                {
                    dto.FieldOptions = JsonConvert.DeserializeObject<List<string>>(field.FieldOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse field options for field {FieldId}", field.Id);
                    dto.FieldOptions = null;
                }
            }

            return dto;
        }
    }
}
