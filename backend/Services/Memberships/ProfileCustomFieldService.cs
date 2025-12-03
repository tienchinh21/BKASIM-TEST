using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Memberships;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Services.Memberships
{
    public class ProfileCustomFieldService : IProfileCustomFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProfileCustomField> _fieldRepository;
        private readonly ILogger<ProfileCustomFieldService> _logger;

        public ProfileCustomFieldService(
            IUnitOfWork unitOfWork,
            IRepository<ProfileCustomField> fieldRepository,
            ILogger<ProfileCustomFieldService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldRepository = fieldRepository;
            _logger = logger;
        }

        public async Task<CustomFieldDto> AddCustomFieldAsync(string profileTemplateId, AddCustomFieldDto dto)
        {
            try
            {
                var field = new ProfileCustomField
                {
                    ProfileTemplateId = profileTemplateId,
                    FieldName = dto.FieldName,
                    FieldValue = dto.FieldValue,
                    FieldType = dto.FieldType,
                    DisplayOrder = dto.DisplayOrder,
                    IsVisible = dto.IsVisible,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _fieldRepository.AddAsync(field);
                await _unitOfWork.SaveChangesAsync();

                return MapToDto(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding custom field to template {TemplateId}", profileTemplateId);
                throw;
            }
        }

        public async Task<List<CustomFieldDto>> GetCustomFieldsAsync(string profileTemplateId)
        {
            try
            {
                var fields = await _fieldRepository.AsQueryable()
                    .Where(f => f.ProfileTemplateId == profileTemplateId)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                return fields.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields for template {TemplateId}", profileTemplateId);
                throw;
            }
        }
        public async Task<List<CustomFieldDto>> GetCustomFieldsByUserZaloIdAsync(string userZaloId)
        {
            try
            {
                var fields = await _fieldRepository.AsQueryable()
                    .Where(f => f.ProfileTemplate.UserZaloId == userZaloId)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();
                return fields.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields for user {UserZaloId}", userZaloId);
                throw;
            }
        }
        public async Task<CustomFieldDto> UpdateCustomFieldAsync(string fieldId, UpdateCustomFieldDto dto)
        {
            try
            {
                var field = await _fieldRepository.FindByIdAsync(fieldId);
                if (field == null)
                    throw new Exception($"Không tìm thấy trường tùy chỉnh {fieldId}");

                if (!string.IsNullOrEmpty(dto.FieldName))
                    field.FieldName = dto.FieldName;

                if (dto.FieldValue != null)
                    field.FieldValue = dto.FieldValue;

                if (!string.IsNullOrEmpty(dto.FieldType))
                    field.FieldType = dto.FieldType;

                if (dto.DisplayOrder.HasValue)
                    field.DisplayOrder = dto.DisplayOrder.Value;

                if (dto.IsVisible.HasValue)
                    field.IsVisible = dto.IsVisible.Value;

                field.UpdatedDate = DateTime.Now;

                _fieldRepository.Update(field);
                await _unitOfWork.SaveChangesAsync();

                return MapToDto(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<bool> DeleteCustomFieldAsync(string fieldId)
        {
            try
            {
                var field = await _fieldRepository.FindByIdAsync(fieldId);
                if (field == null)
                    return false;

                _fieldRepository.Delete(field);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting custom field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<CustomFieldDto?> GetCustomFieldByIdAsync(string fieldId)
        {
            try
            {
                var field = await _fieldRepository.FindByIdAsync(fieldId);
                return field == null ? null : MapToDto(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom field {FieldId}", fieldId);
                throw;
            }
        }

        private CustomFieldDto MapToDto(ProfileCustomField field)
        {
            return new CustomFieldDto
            {
                Id = field.Id,
                ProfileTemplateId = field.ProfileTemplateId,
                FieldName = field.FieldName,
                FieldValue = field.FieldValue,
                FieldType = field.FieldType,
                DisplayOrder = field.DisplayOrder,
                IsVisible = field.IsVisible,
                CreatedDate = field.CreatedDate,
                UpdatedDate = field.UpdatedDate
            };
        }
    }
}
