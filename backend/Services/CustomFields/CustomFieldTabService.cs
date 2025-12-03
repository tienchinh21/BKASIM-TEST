using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service for managing custom field tabs
    /// </summary>
    public class CustomFieldTabService : ICustomFieldTabService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CustomFieldTab> _tabRepository;
        private readonly IRepository<CustomField> _fieldRepository;
        private readonly ILogger<CustomFieldTabService> _logger;

        public CustomFieldTabService(
            IUnitOfWork unitOfWork,
            ILogger<CustomFieldTabService> logger)
        {
            _unitOfWork = unitOfWork;
            _tabRepository = unitOfWork.GetRepository<CustomFieldTab>();
            _fieldRepository = unitOfWork.GetRepository<CustomField>();
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all tabs for a specific entity, ordered by display order
        /// </summary>
        public async Task<List<CustomFieldTabDTO>> GetTabsByEntityAsync(ECustomFieldEntityType entityType, string entityId)
        {
            try
            {
                _logger.LogInformation("Getting tabs for entity type {EntityType}, entity ID {EntityId}", entityType, entityId);

                var tabs = await _tabRepository.AsQueryable()
                    .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDelete)
                    .Include(t => t.CustomFields.Where(f => !f.IsDelete))
                    .OrderBy(t => t.DisplayOrder)
                    .ToListAsync();

                var tabDTOs = tabs.Select(t => new CustomFieldTabDTO
                {
                    Id = t.Id,
                    EntityId = t.EntityId,
                    TabName = t.TabName,
                    DisplayOrder = t.DisplayOrder,
                    FieldCount = t.CustomFields?.Count ?? 0,
                    Fields = t.CustomFields?.OrderBy(f => f.DisplayOrder).Select(f => new CustomFieldDTO
                    {
                        Id = f.Id,
                        CustomFieldTabId = f.CustomFieldTabId,
                        EntityId = f.EntityId,
                        FieldName = f.FieldName,
                        FieldType = f.FieldType,
                        FieldTypeText = f.FieldType.ToString(),
                        FieldOptions = !string.IsNullOrEmpty(f.FieldOptions) 
                            ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(f.FieldOptions) 
                            : null,
                        IsRequired = f.IsRequired,
                        DisplayOrder = f.DisplayOrder,
                        IsProfile = f.IsProfile,
                        CreatedDate = f.CreatedDate,
                        UpdatedDate = f.UpdatedDate
                    }).ToList(),
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate
                }).ToList();

                _logger.LogInformation("Retrieved {TabCount} tabs for entity type {EntityType}, entity ID {EntityId}",
                    tabDTOs.Count, entityType, entityId);

                return tabDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tabs for entity type {EntityType}, entity ID {EntityId}", entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new tab with validation for required fields
        /// </summary>
        public async Task<CustomFieldTabDTO> CreateTabAsync(CreateCustomFieldTabRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.EntityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(request.EntityId));
                }

                if (string.IsNullOrWhiteSpace(request.TabName))
                {
                    throw new ArgumentException("TabName is required", nameof(request.TabName));
                }

                if (request.DisplayOrder < 0)
                {
                    throw new ArgumentException("DisplayOrder must be non-negative", nameof(request.DisplayOrder));
                }

                _logger.LogInformation("Creating new tab for entity type {EntityType}, entity ID {EntityId}, tab name {TabName}",
                    request.EntityType, request.EntityId, request.TabName);

                var tab = new CustomFieldTab
                {
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    TabName = request.TabName,
                    DisplayOrder = request.DisplayOrder,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _tabRepository.AddAsync(tab);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to save tab to database");
                }

                _logger.LogInformation("Successfully created tab with ID {TabId}", tab.Id);

                return new CustomFieldTabDTO
                {
                    Id = tab.Id,
                    EntityId = tab.EntityId,
                    TabName = tab.TabName,
                    DisplayOrder = tab.DisplayOrder,
                    FieldCount = 0,
                    CreatedDate = tab.CreatedDate,
                    UpdatedDate = tab.UpdatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tab with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing tab with round-trip persistence
        /// </summary>
        public async Task<CustomFieldTabDTO> UpdateTabAsync(UpdateCustomFieldTabRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    throw new ArgumentException("Id is required", nameof(request.Id));
                }

                if (string.IsNullOrWhiteSpace(request.TabName))
                {
                    throw new ArgumentException("TabName is required", nameof(request.TabName));
                }

                if (request.DisplayOrder < 0)
                {
                    throw new ArgumentException("DisplayOrder must be non-negative", nameof(request.DisplayOrder));
                }

                _logger.LogInformation("Updating tab with ID {TabId}", request.Id);

                var tab = await _tabRepository.AsQueryable()
                    .Include(t => t.CustomFields)
                    .FirstOrDefaultAsync(t => t.Id == request.Id);

                if (tab == null)
                {
                    throw new KeyNotFoundException($"Tab with ID {request.Id} not found");
                }

                tab.TabName = request.TabName;
                tab.DisplayOrder = request.DisplayOrder;
                tab.UpdatedDate = DateTime.Now;

                _tabRepository.Update(tab);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to update tab in database");
                }

                _logger.LogInformation("Successfully updated tab with ID {TabId}", tab.Id);

                return new CustomFieldTabDTO
                {
                    Id = tab.Id,
                    EntityId = tab.EntityId,
                    TabName = tab.TabName,
                    DisplayOrder = tab.DisplayOrder,
                    FieldCount = tab.CustomFields?.Count ?? 0,
                    CreatedDate = tab.CreatedDate,
                    UpdatedDate = tab.UpdatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tab with request: {@Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Soft deletes a tab and all associated custom fields (sets IsDelete = true)
        /// </summary>
        public async Task<bool> DeleteTabAsync(string tabId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tabId))
                {
                    throw new ArgumentException("TabId is required", nameof(tabId));
                }

                _logger.LogInformation("Soft deleting tab with ID {TabId}", tabId);

                var tab = await _tabRepository.AsQueryable()
                    .Include(t => t.CustomFields)
                    .FirstOrDefaultAsync(t => t.Id == tabId);

                if (tab == null)
                {
                    _logger.LogWarning("Tab with ID {TabId} not found", tabId);
                    return false;
                }

                // Soft delete all associated custom fields
                if (tab.CustomFields != null && tab.CustomFields.Any())
                {
                    _logger.LogInformation("Soft deleting {FieldCount} associated fields for tab {TabId}",
                        tab.CustomFields.Count, tabId);

                    foreach (var field in tab.CustomFields)
                    {
                        field.IsDelete = true;
                        field.UpdatedDate = DateTime.Now;
                    }
                    _fieldRepository.UpdateRange(tab.CustomFields);
                }

                // Soft delete the tab
                tab.IsDelete = true;
                tab.UpdatedDate = DateTime.Now;
                _tabRepository.Update(tab);
                
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to soft delete tab from database");
                }

                _logger.LogInformation("Successfully soft deleted tab with ID {TabId}", tabId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting tab with ID {TabId}", tabId);
                throw;
            }
        }

        /// <summary>
        /// Reorders tabs by updating their display order
        /// </summary>
        public async Task<bool> ReorderTabsAsync(string entityId, List<(string TabId, int Order)> reordering)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    throw new ArgumentException("EntityId is required", nameof(entityId));
                }

                if (reordering == null || !reordering.Any())
                {
                    throw new ArgumentException("Reordering list cannot be empty", nameof(reordering));
                }

                _logger.LogInformation("Reordering {TabCount} tabs for entity ID {EntityId}", reordering.Count, entityId);

                var tabIds = reordering.Select(r => r.TabId).ToList();
                var tabs = await _tabRepository.AsQueryable()
                    .Where(t => tabIds.Contains(t.Id) && t.EntityId == entityId)
                    .ToListAsync();

                if (tabs.Count != reordering.Count)
                {
                    throw new InvalidOperationException("Some tabs not found or do not belong to the specified entity");
                }

                // Update display orders
                foreach (var (tabId, order) in reordering)
                {
                    var tab = tabs.FirstOrDefault(t => t.Id == tabId);
                    if (tab != null)
                    {
                        tab.DisplayOrder = order;
                        tab.UpdatedDate = DateTime.Now;
                    }
                }

                _tabRepository.UpdateRange(tabs);
                var savedCount = await _unitOfWork.SaveChangesAsync();

                if (savedCount == 0)
                {
                    throw new InvalidOperationException("Failed to update tab display orders in database");
                }

                _logger.LogInformation("Successfully reordered {TabCount} tabs for entity ID {EntityId}",
                    reordering.Count, entityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering tabs for entity ID {EntityId}", entityId);
                throw;
            }
        }
    }
}
