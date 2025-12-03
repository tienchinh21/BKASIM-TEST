using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Base.Interface
{
    public interface ICreateService<T>
    {
        Task<int> CreateAsync(T entity);
        Task<int> CreateRangeAsync(IEnumerable<T> entities);
    }

    public interface IUpdateService<T>
    {
        Task<int> UpdateAsync(T entity);
        Task<int> UpdateAsync(string id, T entity);
        Task<int> UpdateRangeAsync(IEnumerable<T> entities);
    }

    public interface IDeleteService<T>
    {
        Task<int> DeleteAsync(T id);
        Task<int> DeleteByIdAsync(string id);
        Task<int> DeleteRangeAsync(IEnumerable<T> entities);
    }

    public interface IReadOnlyService<T>
    {
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<string> ids);
        Task<PagedResult<T>> GetPage(IRequestQuery query);
    }

    public interface IService<T> :
        ICreateService<T>,
        IUpdateService<T>,
        IDeleteService<T>,
        IReadOnlyService<T>
    {
        bool IsAdmin { get; set; }
    }
}
