using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Sponsors;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Sponsors;
using MiniAppGIBA.Models.Queries.Sponsors;
using MiniAppGIBA.Models.Request.Sponsors;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Services.Sponsors
{
    public class SponsorService : ISponsorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Sponsor> _sponsorRepository;
        private readonly ILogger<SponsorService> _logger;
        private readonly IWebHostEnvironment _env;

        public SponsorService(
            IUnitOfWork unitOfWork,
            IRepository<Sponsor> sponsorRepository,
            ILogger<SponsorService> logger,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _sponsorRepository = sponsorRepository;
            _logger = logger;
            _env = env;
        }

        public async Task<PagedResult<SponsorDTO>> GetSponsorsAsync(SponsorQueryParameters query)
        {
            try
            {
                IQueryable<Sponsor> queryable = _sponsorRepository.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(s => s.SponsorName.Contains(query.Keyword));
                }

                if (query.IsActive.HasValue)
                {
                    queryable = queryable.Where(s => s.IsActive == query.IsActive.Value);
                }

                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(s => s.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var sponsorDTOs = items.Select(MapToSponsorDTO).ToList();

                return new PagedResult<SponsorDTO>
                {
                    Items = sponsorDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsors with query: {@Query}", query);
                throw;
            }
        }

        public async Task<SponsorDTO?> GetSponsorByIdAsync(string id)
        {
            try
            {
                var sponsor = await _sponsorRepository.FindByIdAsync(id);
                return sponsor != null ? MapToSponsorDTO(sponsor) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsor by id: {SponsorId}", id);
                throw;
            }
        }

        public async Task<SponsorDTO> CreateSponsorAsync(CreateSponsorRequest request)
        {
            try
            {
                var sponsor = new Sponsor
                {
                    SponsorName = request.SponsorName,
                    Introduction = request.Introduction,
                    WebsiteURL = request.WebsiteURL,
                    IsActive = request.IsActive,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Process image upload
                if (request.Image != null)
                {
                    sponsor.Image = await ProcessImageUpload(request.Image);
                }

                await _sponsorRepository.AddAsync(sponsor);
                await _unitOfWork.SaveChangesAsync();

                return MapToSponsorDTO(sponsor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsor with request: {@Request}", request);
                throw;
            }
        }

        public async Task<SponsorDTO> UpdateSponsorAsync(string id, UpdateSponsorRequest request)
        {
            try
            {
                _logger.LogInformation("🔄 SERVICE: Updating sponsor with ID: '{Id}'", id);
                _logger.LogInformation("🔄 SERVICE REQUEST: SponsorName='{SponsorName}', ShouldRemoveImage={ShouldRemoveImage}", 
                    request.SponsorName, request.ShouldRemoveImage);
                
                var sponsor = await _sponsorRepository.FindByIdAsync(id);
                if (sponsor == null)
                {
                    _logger.LogError("❌ SERVICE: Sponsor with id '{Id}' not found", id);
                    throw new ArgumentException($"Sponsor with id {id} not found");
                }

                _logger.LogInformation("Found existing sponsor: {SponsorName}", sponsor.SponsorName);

                // Update basic fields
                sponsor.SponsorName = request.SponsorName;
                sponsor.Introduction = request.Introduction;
                sponsor.WebsiteURL = request.WebsiteURL;
                sponsor.IsActive = request.IsActive;
                sponsor.UpdatedDate = DateTime.Now;

                // CRITICAL FIX: Handle image removal logic properly
                string? oldImagePath = sponsor.Image;
                
                if (request.ShouldRemoveImage)
                {
                    _logger.LogInformation("Removing existing image: {ImagePath}", oldImagePath);
                    
                    // Remove image path from database
                    sponsor.Image = null;
                    
                    // Delete physical file (do this after DB update succeeds)
                }
                else if (request.Image != null)
                {
                    _logger.LogInformation("Uploading new image to replace existing: {OldImage}", oldImagePath);
                    
                    // Upload new image first
                    string newImagePath = await ProcessImageUpload(request.Image);
                    sponsor.Image = newImagePath;
                    
                    // Mark old image for deletion (do this after DB update succeeds)
                }
                // If neither ShouldRemoveImage nor new Image provided, keep existing image

                // Update in database first
                _sponsorRepository.Update(sponsor);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Database update successful");

                // CRITICAL FIX: Only delete old files AFTER successful DB update
                if ((request.ShouldRemoveImage || request.Image != null) && !string.IsNullOrEmpty(oldImagePath))
                {
                    _logger.LogInformation("Deleting old image file: {OldImagePath}", oldImagePath);
                    DeleteFile(oldImagePath);
                }

                return MapToSponsorDTO(sponsor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsor {SponsorId} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteSponsorAsync(string id)
        {
            try
            {
                var sponsor = await _sponsorRepository.FindByIdAsync(id);
                if (sponsor == null)
                {
                    return false;
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(sponsor.Image))
                {
                    DeleteFile(sponsor.Image);
                }

                _sponsorRepository.Delete(sponsor);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sponsor {SponsorId}", id);
                throw;
            }
        }

        public async Task<List<SponsorDTO>> GetActiveSponsorsAsync()
        {
            try
            {
                var sponsors = await _sponsorRepository.AsQueryable()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SponsorName)
                    .ToListAsync();

                return sponsors.Select(MapToSponsorDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sponsors");
                throw;
            }
        }

        private async Task<string> ProcessImageUpload(IFormFile imageFile)
        {
            var savePath = Path.Combine(_env.WebRootPath, "uploads/images/sponsors");
            var fileName = await FileHandler.SaveFile(imageFile, savePath);
            return $"/uploads/images/sponsors/{fileName}";
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            }
        }

        private SponsorDTO MapToSponsorDTO(Sponsor sponsor)
        {
            return new SponsorDTO
            {
                Id = sponsor.Id,
                SponsorName = sponsor.SponsorName,
                Image = sponsor.Image,
                Introduction = sponsor.Introduction,
                WebsiteURL = sponsor.WebsiteURL,
                IsActive = sponsor.IsActive,
                CreatedDate = sponsor.CreatedDate,
                UpdatedDate = sponsor.UpdatedDate
            };
        }
    }
}
