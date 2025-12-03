using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.HomePins;
using MiniAppGIBA.Enum;

namespace MiniAppGIBA.Services.HomePins
{
    /// <summary>
    /// Repository implementation for HomePin entity
    /// </summary>
    public class HomePinRepository : IHomePinRepository
    {
        private readonly ApplicationDbContext _context;

        public HomePinRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HomePin?> GetByIdAsync(string id)
        {
            return await _context.HomePins
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<HomePin?> GetByEntityAsync(PinEntityType entityType, string entityId)
        {
            return await _context.HomePins
                .FirstOrDefaultAsync(p => p.EntityType == entityType && p.EntityId == entityId);
        }

        public async Task<List<HomePin>> GetAllActiveAsync()
        {
            return await _context.HomePins
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<HomePin>> GetByEntityTypeAsync(PinEntityType entityType)
        {
            return await _context.HomePins
                .Where(p => p.IsActive && p.EntityType == entityType)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> GetActivePinsCountAsync()
        {
            return await _context.HomePins
                .CountAsync(p => p.IsActive);
        }

        public async Task<HomePin> AddAsync(HomePin pin)
        {
            pin.CreatedDate = DateTime.Now;
            pin.UpdatedDate = DateTime.Now;
            
            await _context.HomePins.AddAsync(pin);
            await _context.SaveChangesAsync();
            
            return pin;
        }

        public async Task UpdateAsync(HomePin pin)
        {
            pin.UpdatedDate = DateTime.Now;
            
            _context.HomePins.Update(pin);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var pin = await GetByIdAsync(id);
            if (pin != null)
            {
                _context.HomePins.Remove(pin);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(PinEntityType entityType, string entityId)
        {
            return await _context.HomePins
                .AnyAsync(p => p.EntityType == entityType && p.EntityId == entityId);
        }

        public async Task<int> GetMaxDisplayOrderAsync()
        {
            var maxOrder = await _context.HomePins
                .Where(p => p.IsActive)
                .MaxAsync(p => (int?)p.DisplayOrder);
            
            return maxOrder ?? 0;
        }

        public async Task ReorderPinsAsync(List<HomePin> pins)
        {
            foreach (var pin in pins)
            {
                pin.UpdatedDate = DateTime.Now;
                _context.HomePins.Update(pin);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}
