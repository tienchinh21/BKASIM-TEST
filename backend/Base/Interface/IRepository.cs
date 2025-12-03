using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Base.Interface
{
    public interface IReadOnlyRepository<T>
    {
        IQueryable<T> AsQueryable();
        IQueryable<T> GetQueryable();
        Task<T?> FindByIdAsync(string id);
        Task<T?> GetByIdAsync(long id);
        Task<T?> GetFirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindByIdsAsync(IEnumerable<string> idds);
        Task<int> CountAsync();
        Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(IQueryable<T> query);
        Task<PagedResult<T>> GetPagedAsync(
            System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int page = 1,
            int pageSize = 10);
    }

    public interface IWriteRepository<T>
    {
        void Add(T entity);
        Task AddAsync(T entity);
        void AddRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
    }

    public interface IDeleteRepository<T>
    {
        void Delete(T entities);
        void DeleteRange(IEnumerable<T> entities);
        Task DeleteByIdAsync(string id);
    }

    public interface IRepository<T> :
                     IWriteRepository<T>,
                     IDeleteRepository<T>,
                     IReadOnlyRepository<T>
    {

    }
}
