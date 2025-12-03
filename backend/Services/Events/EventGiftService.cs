using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Events;
using MiniAppGIBA.Models.Queries.Events;
using MiniAppGIBA.Models.Request.Events;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Services.Events
{
    public class EventGiftService : IEventGiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EventGift> _eventGiftRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly ILogger<EventGiftService> _logger;
        private readonly IWebHostEnvironment _env;

        public EventGiftService(
            IUnitOfWork unitOfWork,
            IRepository<EventGift> eventGiftRepository,
            IRepository<Event> eventRepository,
            ILogger<EventGiftService> logger,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _eventGiftRepository = eventGiftRepository;
            _eventRepository = eventRepository;
            _logger = logger;
            _env = env;
        }

        public async Task<PagedResult<EventGiftDTO>> GetEventGiftsAsync(EventGiftQueryParameters query)
        {
            try
            {
                // Build query with Include
                IQueryable<EventGift> queryable = _eventGiftRepository.AsQueryable()
                    .Include(g => g.Event);

                // Apply filters
                if (!string.IsNullOrEmpty(query.Keyword))
                {
                    queryable = queryable.Where(g => g.GiftName.Contains(query.Keyword));
                }

                if (!string.IsNullOrEmpty(query.EventId))
                {
                    queryable = queryable.Where(g => g.EventId == query.EventId);
                }

                // Count total items
                var totalItems = await queryable.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                // Apply ordering and paging
                var items = await queryable
                    .OrderByDescending(g => g.CreatedDate)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var giftDTOs = items.Select(MapToEventGiftDTO).ToList();

                return new PagedResult<EventGiftDTO>
                {
                    Items = giftDTOs,
                    TotalItems = totalItems,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event gifts with query: {@Query}", query);
                throw;
            }
        }

        public async Task<EventGiftDTO?> GetEventGiftByIdAsync(string id)
        {
            try
            {
                var gift = await _eventGiftRepository.AsQueryable()
                    .Include(g => g.Event)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gift == null)
                {
                    return null;
                }

                return MapToEventGiftDTO(gift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event gift by id: {GiftId}", id);
                throw;
            }
        }

        public async Task<EventGiftDTO> CreateEventGiftAsync(CreateEventGiftRequest request)
        {
            try
            {
                // Verify event exists
                var eventEntity = await _eventRepository.FindByIdAsync(request.EventId);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy sự kiện");
                }

                var gift = new EventGift
                {
                    EventId = request.EventId,
                    GiftName = request.GiftName,
                    Quantity = request.Quantity,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Process images upload
                if (request.ImageFiles != null && request.ImageFiles.Any())
                {
                    gift.Images = await ProcessImageUpload(request.ImageFiles);
                }

                await _eventGiftRepository.AddAsync(gift);
                await _unitOfWork.SaveChangesAsync();

                // Load with navigation properties for DTO mapping
                var createdGift = await _eventGiftRepository.AsQueryable()
                    .Include(g => g.Event)
                    .FirstOrDefaultAsync(g => g.Id == gift.Id);

                return MapToEventGiftDTO(createdGift!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event gift with request: {@Request}", request);
                throw;
            }
        }

        public async Task<EventGiftDTO> UpdateEventGiftAsync(string id, UpdateEventGiftRequest request)
        {
            try
            {
                var gift = await _eventGiftRepository.AsQueryable()
                    .Include(g => g.Event)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gift == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy phần quà");
                }

                // Verify event exists
                var eventEntity = await _eventRepository.FindByIdAsync(request.EventId);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy sự kiện");
                }

                gift.EventId = request.EventId;
                gift.GiftName = request.GiftName;
                gift.Quantity = request.Quantity;
                gift.UpdatedDate = DateTime.Now;

                // Process images upload
                if (request.ImageFiles != null && request.ImageFiles.Any())
                {
                    // Delete old images if exists
                    if (!string.IsNullOrEmpty(gift.Images))
                    {
                        var oldImages = gift.Images.Split(',');
                        foreach (var oldImage in oldImages)
                        {
                            DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/gifts", oldImage.Trim()));
                        }
                    }
                    gift.Images = await ProcessImageUpload(request.ImageFiles);
                }

                _eventGiftRepository.Update(gift);
                await _unitOfWork.SaveChangesAsync();

                return MapToEventGiftDTO(gift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event gift {GiftId} with request: {@Request}", id, request);
                throw;
            }
        }

        public async Task<bool> DeleteEventGiftAsync(string id)
        {
            try
            {
                var gift = await _eventGiftRepository.FindByIdAsync(id);
                if (gift == null)
                {
                    return false;
                }

                // Delete associated files
                if (!string.IsNullOrEmpty(gift.Images))
                {
                    var images = gift.Images.Split(',');
                    foreach (var image in images)
                    {
                        DeleteFile(Path.Combine(_env.WebRootPath, "uploads/images/gifts", image.Trim()));
                    }
                }

                _eventGiftRepository.Delete(gift);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event gift {GiftId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleEventGiftStatusAsync(string id)
        {
            try
            {
                var gift = await _eventGiftRepository.FindByIdAsync(id);
                if (gift == null)
                {
                    return false;
                }

                gift.UpdatedDate = DateTime.Now;

                _eventGiftRepository.Update(gift);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling event gift status {GiftId}", id);
                throw;
            }
        }

        public async Task<List<EventGiftDTO>> GetGiftsByEventAsync(string eventId)
        {
            try
            {
                var gifts = await _eventGiftRepository.AsQueryable()
                    .Include(g => g.Event)
                    .Where(g => g.EventId == eventId && true)
                    .OrderBy(g => g.GiftName)
                    .ToListAsync();

                return gifts.Select(MapToEventGiftDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gifts by event {EventId}", eventId);
                throw;
            }
        }

        #region Private Methods

        private async Task<string> ProcessImageUpload(List<IFormFile> imageFiles)
        {
            var savePath = Path.Combine(_env.WebRootPath, "uploads/images/gifts");
            var fileNames = new List<string>();

            foreach (var file in imageFiles)
            {
                var fileName = await FileHandler.SaveFile(file, savePath);
                fileNames.Add(fileName);
            }

            return string.Join(",", fileNames);
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            }
        }

        private EventGiftDTO MapToEventGiftDTO(EventGift gift)
        {
            return new EventGiftDTO
            {
                Id = gift.Id,
                EventId = gift.EventId,
                EventTitle = gift.Event?.Title ?? "N/A",
                GiftName = gift.GiftName,
                Images = string.IsNullOrEmpty(gift.Images)
                    ? new List<string>()
                    : new List<string> { gift.Images },
                Quantity = gift.Quantity,
                CreatedDate = gift.CreatedDate,
                UpdatedDate = gift.UpdatedDate
            };
        }

        #endregion
    }
}
