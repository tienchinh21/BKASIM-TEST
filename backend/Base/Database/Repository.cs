using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;
using System.Linq.Expressions;

namespace MiniAppGIBA.Base.Database
{
    public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
    {
        #region "READ"

        public IQueryable<T> AsQueryable()
        {
            return context.Set<T>().AsQueryable();
        }

        public IQueryable<T> GetQueryable()
        {
            return context.Set<T>().AsQueryable();
        }

        public async Task<T?> FindByIdAsync(string id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Enumerable.Empty<T>();
            }

            return await context.Set<T>()
                .AsNoTracking()
                .Where(e => ids.Contains(EF.Property<string>(e, "Id")))
                .ToListAsync();
        }

        #endregion

        #region "CREATE"

        public void Add(T entity)
        {
            context.Set<T>().Add(entity);
            //return context.SaveChangesAsync();
        }

        public async Task AddAsync(T entity)
        {
            await context.Set<T>().AddAsync(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            context.Set<T>().AddRange(entities);
            //return await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await context.Set<T>().AddRangeAsync(entities);
        }

        #endregion

        #region "UPDATE"

        public void Update(T entity)
        {
            context.Set<T>().Update(entity);
            //return await context.SaveChangesAsync();
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            context.Set<T>().UpdateRange(entities);
            //return await context.SaveChangesAsync();
        }
        #endregion

        #region "DELETE"

        public void Delete(T entity)
        {
            context.Set<T>().Remove(entity);
            //await context.SaveChangesAsync();
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            context.Set<T>().RemoveRange(entities);
            //await context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(string id)
        {
            var item = await FindByIdAsync(id);
            if (item != null)
            {
                context.Set<T>().Remove(item);
            }
            //await context.SaveChangesAsync();
        }

        #endregion

        #region "ADDITIONAL METHODS"

        public async Task<int> CountAsync()
        {
            return await context.Set<T>().CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().CountAsync(predicate);
        }

        public async Task<bool> AnyAsync(IQueryable<T> query)
        {
            return await query.AnyAsync();
        }

        public async Task<PagedResult<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = context.Set<T>().AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        #endregion
    }
}
