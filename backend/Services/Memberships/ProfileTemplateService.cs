using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Memberships;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Services.Memberships
{
    public class ProfileTemplateService : IProfileTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProfileTemplate> _templateRepository;
        private readonly ILogger<ProfileTemplateService> _logger;

        public ProfileTemplateService(
            IUnitOfWork unitOfWork,
            IRepository<ProfileTemplate> templateRepository,
            ILogger<ProfileTemplateService> logger)
        {
            _unitOfWork = unitOfWork;
            _templateRepository = templateRepository;
            _logger = logger;
        }

        public async Task<GetProfileTemplateDto?> GetTemplateAsync(string userZaloId)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .Include(t => t.CustomFields)
                    .FirstOrDefaultAsync();

                if (template == null)
                    return null;

                return MapToDto(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<GetProfileTemplateDto> CreateOrUpdateTemplateAsync(string userZaloId, UpdateProfileTemplateDto dto)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    template = new ProfileTemplate
                    {
                        UserZaloId = userZaloId,
                        VisibleFields = dto.VisibleFields != null ? JsonSerializer.Serialize(dto.VisibleFields) : null,
                        HiddenFields = dto.HiddenFields != null ? JsonSerializer.Serialize(dto.HiddenFields) : null,
                        CustomDescription = dto.CustomDescription,
                        ThemeColor = dto.ThemeColor,
                        IsPublic = dto.IsPublic,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _templateRepository.AddAsync(template);
                }
                else
                {
                    template.VisibleFields = dto.VisibleFields != null ? JsonSerializer.Serialize(dto.VisibleFields) : template.VisibleFields;
                    template.HiddenFields = dto.HiddenFields != null ? JsonSerializer.Serialize(dto.HiddenFields) : template.HiddenFields;
                    template.CustomDescription = dto.CustomDescription ?? template.CustomDescription;
                    template.ThemeColor = dto.ThemeColor ?? template.ThemeColor;
                    template.IsPublic = dto.IsPublic;
                    template.UpdatedDate = DateTime.Now;

                    _templateRepository.Update(template);
                }

                await _unitOfWork.SaveChangesAsync();

                var result = await _templateRepository.AsQueryable()
                    .Where(t => t.Id == template.Id)
                    .Include(t => t.CustomFields)
                    .FirstOrDefaultAsync();

                return MapToDto(result!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating template for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<List<string>> GetVisibleFieldsAsync(string userZaloId)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (template == null || string.IsNullOrEmpty(template.VisibleFields))
                    return new List<string>();

                return JsonSerializer.Deserialize<List<string>>(template.VisibleFields) ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visible fields for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<List<string>> GetHiddenFieldsAsync(string userZaloId)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (template == null || string.IsNullOrEmpty(template.HiddenFields))
                    return new List<string>();

                return JsonSerializer.Deserialize<List<string>>(template.HiddenFields) ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hidden fields for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<string> UpdateCoverImageAsync(string userZaloId, string imagePath)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    template = new ProfileTemplate
                    {
                        UserZaloId = userZaloId,
                        CoverImage = imagePath,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    await _templateRepository.AddAsync(template);
                }
                else
                {
                    template.CoverImage = imagePath;
                    template.UpdatedDate = DateTime.Now;
                    _templateRepository.Update(template);
                }

                await _unitOfWork.SaveChangesAsync();
                return imagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cover image for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<bool> DeleteTemplateAsync(string userZaloId)
        {
            try
            {
                var template = await _templateRepository.AsQueryable()
                    .Where(t => t.UserZaloId == userZaloId)
                    .FirstOrDefaultAsync();

                if (template == null)
                    return false;

                _templateRepository.Delete(template);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        private GetProfileTemplateDto MapToDto(ProfileTemplate template)
        {
            var visibleFields = string.IsNullOrEmpty(template.VisibleFields)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(template.VisibleFields) ?? new List<string>();

            var hiddenFields = string.IsNullOrEmpty(template.HiddenFields)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(template.HiddenFields) ?? new List<string>();

            return new GetProfileTemplateDto
            {
                Id = template.Id,
                UserZaloId = template.UserZaloId,
                VisibleFields = visibleFields,
                HiddenFields = hiddenFields,
                CustomDescription = template.CustomDescription,
                CoverImage = template.CoverImage,
                ThemeColor = template.ThemeColor,
                IsPublic = template.IsPublic,
                CreatedDate = template.CreatedDate,
                UpdatedDate = template.UpdatedDate,
                CustomFields = template.CustomFields?.Select(cf => new CustomFieldDto
                {
                    Id = cf.Id,
                    ProfileTemplateId = cf.ProfileTemplateId,
                    FieldName = cf.FieldName,
                    FieldValue = cf.FieldValue,
                    FieldType = cf.FieldType,
                    DisplayOrder = cf.DisplayOrder,
                    IsVisible = cf.IsVisible,
                    CreatedDate = cf.CreatedDate,
                    UpdatedDate = cf.UpdatedDate
                }).ToList()
            };
        }
    }
}
