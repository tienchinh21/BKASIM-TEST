using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;

namespace MiniAppGIBA.Services.OmniTool.MappingData
{
    public class MappingDataService(ApplicationDbContext context) : IMappingDataService
    {
        private IQueryable<object>? GetTable(string tableName)
        {
            var property = context.GetType()
                                  .GetProperties()
                                  .FirstOrDefault(p => p.Name.ToLower() == tableName.ToLower());
            return property?.GetValue(this) as IQueryable<object>;
        }

        public async Task<Dictionary<string, object>?> GetDataAsync(string tableName, string columnName, Dictionary<string, object>? filters = null)
        {
            var query = GetTable(tableName);
            if (query == null)
            {
                return null;
            }
            // Áp dụng bộ lọc nếu có
            foreach (var filter in filters ?? new Dictionary<string, object>())
            {
                query = query.Where(e => EF.Property<object>(e, filter.Key) == filter.Value);
            }
            // Lấy dữ liệu từ columnName được chỉ định
            var entity = await query.FirstOrDefaultAsync();
            if (entity == null)
            {
                return null;
            }
            var result = new Dictionary<string, object>
            {
                { columnName, EF.Property<object>(entity, columnName) }
            };
            return result;
        }
    }
}
