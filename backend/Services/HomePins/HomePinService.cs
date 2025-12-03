using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.HomePins;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.HomePins;

namespace MiniAppGIBA.Services.HomePins
{
    public class HomePinService : IHomePinService
    {
        private readonly IHomePinRepository _homePinRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        private int MaxPinsAllowed => _configuration.GetValue<int>("HomePins:MaxAllowed", 10);

        public HomePinService(
            IHomePinRepository homePinRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _homePinRepository = homePinRepository;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<Result<HomePinDto>> PinEntityAsync(CreateHomePinRequest request, string adminId)
        {
            var entityExists = await ValidateEntityExistsAsync(request.EntityType, request.EntityId);
            if (!entityExists.IsSuccess)
                return Result<HomePinDto>.Failure(entityExists.Message);

            var existing = await _homePinRepository.GetByEntityAsync(request.EntityType, request.EntityId);
            if (existing != null)
                return Result<HomePinDto>.Failure("Mục này đã được ghim trước đó");

            var currentCount = await _homePinRepository.GetActivePinsCountAsync();
            if (currentCount >= MaxPinsAllowed)
                return Result<HomePinDto>.Failure($"Đã đạt giới hạn số lượng ghim tối đa ({MaxPinsAllowed})");

            var maxOrder = await _homePinRepository.GetMaxDisplayOrderAsync();
            var pin = new HomePin
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                DisplayOrder = maxOrder + 1,
                PinnedBy = adminId,
                PinnedAt = DateTime.Now,
                Notes = request.Notes,
                IsActive = true
            };

            await _homePinRepository.AddAsync(pin);

            var dto = await MapToDtoAsync(pin);
            return Result<HomePinDto>.Success(dto, "Ghim thành công");
        }

        public async Task<Result<bool>> UnpinEntityAsync(string pinId, string adminId)
        {
            var pin = await _homePinRepository.GetByIdAsync(pinId);
            if (pin == null)
                return Result<bool>.Failure("Không tìm thấy mục ghim");

            await _homePinRepository.DeleteAsync(pinId);

            var remainingPins = await _homePinRepository.GetAllActiveAsync();
            for (int i = 0; i < remainingPins.Count; i++)
            {
                remainingPins[i].DisplayOrder = i + 1;
            }
            await _homePinRepository.ReorderPinsAsync(remainingPins);

            return Result<bool>.Success(true, "Gỡ ghim thành công");
        }

        public async Task<Result<HomePinListResponse>> GetHomePinsAsync(PinEntityType? filterType = null)
        {
            // Get total count of ALL pins (for limit checking)
            var totalPinsCount = await _homePinRepository.GetActivePinsCountAsync();

            // Get filtered pins
            var pins = filterType.HasValue
                ? await _homePinRepository.GetByEntityTypeAsync(filterType.Value)
                : await _homePinRepository.GetAllActiveAsync();

            var dtos = new List<HomePinDto>();
            foreach (var pin in pins)
            {
                var dto = await MapToDtoAsync(pin);
                dtos.Add(dto);
            }

            var response = new HomePinListResponse
            {
                Pins = dtos,
                TotalCount = dtos.Count, // Filtered count
                TotalPinsCount = totalPinsCount, // Total count (all types)
                MaxPinsAllowed = MaxPinsAllowed,
                CanAddMore = totalPinsCount < MaxPinsAllowed // Check against total, not filtered
            };

            return Result<HomePinListResponse>.Success(response);
        }

        public async Task<Result<HomePinListResponse>> GetHomePinsForUserAsync(string? userZaloId, PinEntityType? filterType = null)
        {
            var pins = filterType.HasValue
                ? await _homePinRepository.GetByEntityTypeAsync(filterType.Value)
                : await _homePinRepository.GetAllActiveAsync();

            // Get user's group memberships if user is authenticated
            var userGroupIds = new HashSet<string>();
            if (!string.IsNullOrEmpty(userZaloId))
            {
                var groupIdsList = await _context.MembershipGroups
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .Select(mg => mg.GroupId)
                    .ToListAsync();
                userGroupIds = groupIdsList.ToHashSet();
            }

            var filteredDtos = new List<HomePinDto>();

            foreach (var pin in pins)
            {
                bool canView = false;

                switch (pin.EntityType)
                {
                    case PinEntityType.Event:
                        var eventEntity = await _context.Events
                            .Where(e => e.Id == pin.EntityId)
                            .Select(e => new { e.Type, e.GroupId })
                            .FirstOrDefaultAsync();

                        if (eventEntity != null)
                        {
                            // Type: 1 = Nội bộ, 2 = Công khai
                            if (eventEntity.Type == 2)
                            {
                                canView = true; // Công khai - everyone can view
                            }
                            else if (eventEntity.Type == 1 && !string.IsNullOrEmpty(userZaloId))
                            {
                                canView = userGroupIds.Contains(eventEntity.GroupId); // Nội bộ - only group members
                            }
                        }
                        break;

                    case PinEntityType.Meeting:
                        var meeting = await _context.Meetings
                            .Where(m => m.Id == pin.EntityId)
                            .Select(m => new { m.IsPublic, m.GroupId })
                            .FirstOrDefaultAsync();

                        if (meeting != null)
                        {
                            if (meeting.IsPublic)
                            {
                                canView = true; // Công khai - everyone can view
                            }
                            else if (!string.IsNullOrEmpty(userZaloId))
                            {
                                canView = userGroupIds.Contains(meeting.GroupId); // Nội bộ - only group members
                            }
                        }
                        break;

                    case PinEntityType.Showcase:
                        var showcase = await _context.Showcases
                            .Where(s => s.Id == pin.EntityId)
                            .Select(s => new { s.IsPublic, s.GroupId })
                            .FirstOrDefaultAsync();

                        if (showcase != null)
                        {
                            if (showcase.IsPublic)
                            {
                                canView = true; // Công khai - everyone can view
                            }
                            else if (!string.IsNullOrEmpty(userZaloId))
                            {
                                canView = userGroupIds.Contains(showcase.GroupId); // Nội bộ - only group members
                            }
                        }
                        break;

                    case PinEntityType.Article:
                        var article = await _context.Articles
                            .Where(a => a.Id == pin.EntityId)
                            .Select(a => new { a.Status, a.GroupIds })
                            .FirstOrDefaultAsync();

                        if (article != null)
                        {
                            // Status: 0 = Nội bộ, 1 = Công khai
                            if (article.Status == 1)
                            {
                                canView = true; // Công khai - everyone can view
                            }
                            else if (article.Status == 0 && !string.IsNullOrEmpty(userZaloId) && !string.IsNullOrEmpty(article.GroupIds))
                            {
                                var articleGroupIds = article.GroupIds.Split(',').Select(g => g.Trim()).ToList();
                                canView = articleGroupIds.Any(gid => userGroupIds.Contains(gid)); // Nội bộ - only group members
                            }
                        }
                        break;
                }

                if (canView)
                {
                    var dto = await MapToDtoAsync(pin);
                    filteredDtos.Add(dto);
                }
            }

            // Get total count of ALL pins (for admin reference)
            var totalPinsCount = await _homePinRepository.GetActivePinsCountAsync();

            var response = new HomePinListResponse
            {
                Pins = filteredDtos,
                TotalCount = filteredDtos.Count, // Filtered count (visible to user)
                TotalPinsCount = totalPinsCount, // Total count (all pins in system)
                MaxPinsAllowed = MaxPinsAllowed,
                CanAddMore = totalPinsCount < MaxPinsAllowed // Check against total
            };

            return Result<HomePinListResponse>.Success(response);
        }

        public async Task<Result<bool>> ReorderPinsAsync(ReorderPinsRequest request, string adminId)
        {
            var pin = await _homePinRepository.GetByIdAsync(request.PinId);
            if (pin == null)
                return Result<bool>.Failure("Không tìm thấy mục ghim");

            var allPins = await _homePinRepository.GetAllActiveAsync();
            var oldOrder = pin.DisplayOrder;
            var newOrder = request.NewDisplayOrder;

            if (newOrder < 1 || newOrder > allPins.Count)
                return Result<bool>.Failure("Thứ tự hiển thị không hợp lệ");

            if (oldOrder == newOrder)
                return Result<bool>.Success(true, "Không có thay đổi");

            if (oldOrder < newOrder)
            {
                foreach (var p in allPins.Where(p => p.DisplayOrder > oldOrder && p.DisplayOrder <= newOrder))
                {
                    p.DisplayOrder--;
                }
            }
            else
            {
                foreach (var p in allPins.Where(p => p.DisplayOrder >= newOrder && p.DisplayOrder < oldOrder))
                {
                    p.DisplayOrder++;
                }
            }

            pin.DisplayOrder = newOrder;
            await _homePinRepository.ReorderPinsAsync(allPins);

            return Result<bool>.Success(true, "Sắp xếp thành công");
        }

        public async Task<Result<HomePinDto>> UpdatePinNotesAsync(string pinId, string notes, string adminId)
        {
            var pin = await _homePinRepository.GetByIdAsync(pinId);
            if (pin == null)
                return Result<HomePinDto>.Failure("Không tìm thấy mục ghim");

            pin.Notes = notes;
            await _homePinRepository.UpdateAsync(pin);

            var dto = await MapToDtoAsync(pin);
            return Result<HomePinDto>.Success(dto, "Cập nhật ghi chú thành công");
        }

        public async Task<Result<bool>> ValidateEntityExistsAsync(PinEntityType entityType, string entityId)
        {
            bool exists = entityType switch
            {
                PinEntityType.Event => await _context.Events.AnyAsync(e => e.Id == entityId && e.IsActive),
                PinEntityType.Meeting => await _context.Meetings.AnyAsync(m => m.Id == entityId),
                PinEntityType.Showcase => await _context.Showcases.AnyAsync(s => s.Id == entityId),
                PinEntityType.Article => await _context.Articles.AnyAsync(a => a.Id == entityId),
                _ => false
            };

            return exists
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Entity không tồn tại hoặc đã bị xóa");
        }

        private async Task<HomePinDto> MapToDtoAsync(HomePin pin)
        {
            var admin = await _userManager.FindByIdAsync(pin.PinnedBy);
            var adminName = admin?.UserName ?? "Unknown";

            object? entityDetails = pin.EntityType switch
            {
                PinEntityType.Event => await _context.Events
                    .Where(e => e.Id == pin.EntityId)
                    .Select(e => new { e.Id, e.Title, e.StartTime, e.EndTime, e.Banner, e.Status })
                    .FirstOrDefaultAsync(),
                PinEntityType.Meeting => await _context.Meetings
                    .Where(m => m.Id == pin.EntityId)
                    .Select(m => new { m.Id, m.Title, m.StartDate, m.EndDate, m.Status })
                    .FirstOrDefaultAsync(),
                PinEntityType.Showcase => await _context.Showcases
                    .Where(s => s.Id == pin.EntityId)
                    .Select(s => new { s.Id, s.Title, s.StartDate, s.EndDate, s.Status })
                    .FirstOrDefaultAsync(),
                PinEntityType.Article => await _context.Articles
                    .Where(a => a.Id == pin.EntityId)
                    .Select(a => new { a.Id, a.Title, a.BannerImage, a.Status, a.CreatedDate })
                    .FirstOrDefaultAsync(),
                _ => null
            };

            return new HomePinDto
            {
                Id = pin.Id,
                EntityType = pin.EntityType,
                EntityTypeName = pin.EntityType.ToString(),
                EntityId = pin.EntityId,
                DisplayOrder = pin.DisplayOrder,
                PinnedBy = pin.PinnedBy,
                PinnedByName = adminName,
                PinnedAt = pin.PinnedAt,
                Notes = pin.Notes,
                EntityDetails = entityDetails
            };
        }

        public async Task<Result<List<object>>> GetAvailableEntitiesForAdminAsync(PinEntityType entityType)
        {
            var entities = new List<object>();

            try
            {
                switch (entityType)
                {
                    case PinEntityType.Event:
                        var events = await _context.Events
                            .Where(e => e.IsActive)
                            .OrderByDescending(e => e.CreatedDate)
                            .Select(e => new { e.Id, e.Title, e.StartTime, e.Type, e.GroupId })
                            .Take(100)
                            .ToListAsync();
                        entities = events.Cast<object>().ToList();
                        break;

                    case PinEntityType.Meeting:
                        var meetings = await _context.Meetings
                            .OrderByDescending(m => m.CreatedDate)
                            .Select(m => new { m.Id, m.Title, m.StartDate, m.IsPublic, m.GroupId })
                            .Take(100)
                            .ToListAsync();
                        entities = meetings.Cast<object>().ToList();
                        break;

                    case PinEntityType.Showcase:
                        var showcases = await _context.Showcases
                            .OrderByDescending(s => s.CreatedDate)
                            .Select(s => new { s.Id, s.Title, s.StartDate, s.IsPublic, s.GroupId })
                            .Take(100)
                            .ToListAsync();
                        entities = showcases.Cast<object>().ToList();
                        break;

                    case PinEntityType.Article:
                        var articles = await _context.Articles
                            .OrderByDescending(a => a.CreatedDate)
                            .Select(a => new { a.Id, a.Title, a.BannerImage, a.Status, a.GroupIds })
                            .Take(100)
                            .ToListAsync();
                        entities = articles.Cast<object>().ToList();
                        break;
                }

                return Result<List<object>>.Success(entities);
            }
            catch (Exception ex)
            {
                return Result<List<object>>.Failure($"Lỗi khi tải danh sách: {ex.Message}");
            }
        }
    }
}
