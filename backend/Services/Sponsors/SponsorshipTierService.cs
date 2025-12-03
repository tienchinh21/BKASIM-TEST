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
    public class SponsorshipTierService : ISponsorshipTierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SponsorshipTier> _sponsorshipTierRepository;
        private readonly ILogger<SponsorshipTierService> _logger;
        private readonly IWebHostEnvironment _env;

        public SponsorshipTierService(
            IUnitOfWork unitOfWork,
            IRepository<SponsorshipTier> sponsorshipTierRepository,
            ILogger<SponsorshipTierService> logger,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _sponsorshipTierRepository = sponsorshipTierRepository;
            _logger = logger;
            _env = env;
        }

        public async Task<PagedResult<SponsorshipTierDTO>> GetSponsorshipTiersAsync(SponsorshipTierQueryParameters query)
        {
            try
            {
                IQueryable<SponsorshipTier> queryable = _sponsorshipTierRepository.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(st => st.TierName.Contains(query.Keyword));
                }

                if (query.IsActive.HasValue)
                {
                    queryable = queryable.Where(st => st.IsActive == query.IsActive.Value);
                }

                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(st => st.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var sponsorshipTierDTOs = items.Select(MapToSponsorshipTierDTO).ToList();

                return new PagedResult<SponsorshipTierDTO>
                {
                    Items = sponsorshipTierDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsorship tiers with query: {@Query}", query);
                throw;
            }
        }

        public async Task<SponsorshipTierDTO?> GetSponsorshipTierByIdAsync(string id)
        {
            try
            {
                var sponsorshipTier = await _sponsorshipTierRepository.FindByIdAsync(id);
                return sponsorshipTier != null ? MapToSponsorshipTierDTO(sponsorshipTier) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsorship tier by id: {SponsorshipTierId}", id);
                throw;
            }
        }

        public async Task<SponsorshipTierDTO> CreateSponsorshipTierAsync(CreateSponsorshipTierRequest request)
        {
            try
            {
                _logger.LogInformation("Starting to create sponsorship tier: {TierName}", request.TierName);

                var sponsorshipTier = new SponsorshipTier
                {
                    TierName = request.TierName,
                    Description = request.Description,
                    IsActive = request.IsActive,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _logger.LogInformation("Created sponsorship tier object, processing image upload...");

                // Process image upload
                if (request.Image != null)
                {
                    _logger.LogInformation("Processing image upload for file: {FileName}, Size: {FileSize} bytes",
                        request.Image.FileName, request.Image.Length);
                    sponsorshipTier.Image = await ProcessImageUpload(request.Image);
                    _logger.LogInformation("Image upload completed: {ImagePath}", sponsorshipTier.Image);
                }
                else
                {
                    _logger.LogInformation("No image provided for sponsorship tier");
                }

                _logger.LogInformation("Adding sponsorship tier to repository...");
                await _sponsorshipTierRepository.AddAsync(sponsorshipTier);

                _logger.LogInformation("Saving changes to database...");
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Sponsorship tier created successfully with ID: {Id}", sponsorshipTier.Id);
                return MapToSponsorshipTierDTO(sponsorshipTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsorship tier with request: {@Request}", request);
                throw;
            }
        }

        public async Task<SponsorshipTierDTO> UpdateSponsorshipTierAsync(string id, UpdateSponsorshipTierRequest request)
        {
            try
            {
                _logger.LogInformation("Updating sponsorship tier with ID: {Id}", id);
                
                var sponsorshipTier = await _sponsorshipTierRepository.FindByIdAsync(id);
                if (sponsorshipTier == null)
                {
                    _logger.LogError("SponsorshipTier with id {Id} not found", id);
                    throw new ArgumentException($"SponsorshipTier with id {id} not found");
                }

                _logger.LogInformation("Found existing sponsorship tier: {TierName}", sponsorshipTier.TierName);

                // Update basic fields
                sponsorshipTier.TierName = request.TierName;
                sponsorshipTier.Description = request.Description;
                sponsorshipTier.IsActive = request.IsActive;
                sponsorshipTier.UpdatedDate = DateTime.Now;

                // CRITICAL FIX: Handle image removal logic properly
                string? oldImagePath = sponsorshipTier.Image;
                
                if (request.ShouldRemoveImage)
                {
                    _logger.LogInformation("Removing existing image: {ImagePath}", oldImagePath);
                    
                    // Remove image path from database
                    sponsorshipTier.Image = null;
                    
                    // Delete physical file (do this after DB update succeeds)
                }
                else if (request.Image != null)
                {
                    _logger.LogInformation("Uploading new image to replace existing: {OldImage}", oldImagePath);
                    
                    // Upload new image first
                    string newImagePath = await ProcessImageUpload(request.Image);
                    sponsorshipTier.Image = newImagePath;
                    
                    // Mark old image for deletion (do this after DB update succeeds)
                }
                // If neither ShouldRemoveImage nor new Image provided, keep existing image

                // Update in database first
                _sponsorshipTierRepository.Update(sponsorshipTier);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Database update successful");

                // CRITICAL FIX: Only delete old files AFTER successful DB update
                if ((request.ShouldRemoveImage || request.Image != null) && !string.IsNullOrEmpty(oldImagePath))
                {
                    _logger.LogInformation("Deleting old image file: {OldImagePath}", oldImagePath);
                    DeleteFile(oldImagePath);
                }

                return MapToSponsorshipTierDTO(sponsorshipTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsorship tier {SponsorshipTierId} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteSponsorshipTierAsync(string id)
        {
            try
            {
                var sponsorshipTier = await _sponsorshipTierRepository.FindByIdAsync(id);
                if (sponsorshipTier == null)
                {
                    return false;
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(sponsorshipTier.Image))
                {
                    DeleteFile(sponsorshipTier.Image);
                }

                _sponsorshipTierRepository.Delete(sponsorshipTier);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sponsorship tier {SponsorshipTierId}", id);
                throw;
            }
        }

        public async Task<List<SponsorshipTierDTO>> GetActiveSponsorshipTiersAsync()
        {
            try
            {
                var sponsorshipTiers = await _sponsorshipTierRepository.AsQueryable()
                    .Where(st => st.IsActive)
                    .OrderBy(st => st.TierName)
                    .ToListAsync();

                return sponsorshipTiers.Select(MapToSponsorshipTierDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sponsorship tiers");
                throw;
            }
        }

        private async Task<string> ProcessImageUpload(IFormFile imageFile)
        {
            try
            {
                var savePath = Path.Combine(_env.WebRootPath, "uploads/images/sponsorship-tiers");
                _logger.LogInformation("Processing image upload. WebRootPath: {WebRootPath}, SavePath: {SavePath}", _env.WebRootPath, savePath);

                // Kiểm tra quyền ghi thư mục
                if (!Directory.Exists(savePath))
                {
                    _logger.LogInformation("Creating directory: {SavePath}", savePath);
                    Directory.CreateDirectory(savePath);
                }

                // Kiểm tra quyền ghi
                var testFile = Path.Combine(savePath, "test_write.tmp");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    _logger.LogInformation("Write permission verified for directory: {SavePath}", savePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No write permission for directory: {SavePath}", savePath);
                    throw new UnauthorizedAccessException($"No write permission for directory: {savePath}", ex);
                }

                _logger.LogInformation("Calling FileHandler.SaveFile with file: {FileName}, Size: {FileSize}",
                    imageFile.FileName, imageFile.Length);

                var fileName = await FileHandler.SaveFile(imageFile, savePath);
                var result = $"/uploads/images/sponsorship-tiers/{fileName}";

                _logger.LogInformation("Image uploaded successfully: {Result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image upload. WebRootPath: {WebRootPath}, FileName: {FileName}",
                    _env.WebRootPath, imageFile?.FileName);
                throw;
            }
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

        private SponsorshipTierDTO MapToSponsorshipTierDTO(SponsorshipTier sponsorshipTier)
        {
            return new SponsorshipTierDTO
            {
                Id = sponsorshipTier.Id,
                TierName = sponsorshipTier.TierName,
                Description = sponsorshipTier.Description,
                Image = sponsorshipTier.Image,
                IsActive = sponsorshipTier.IsActive,
                CreatedDate = sponsorshipTier.CreatedDate,
                UpdatedDate = sponsorshipTier.UpdatedDate
            };
        }
    }
}
