using MiniAppGIBA.Base.Database;

namespace MiniAppGIBA.Base.Interface
{
    public interface IUnitOfWork
    {
        ApplicationDbContext Context { get; }
        IRepository<T> GetRepository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);

        // Thêm phương thức Bulk Insert
        Task BulkInsertAsync<T>(IList<T> entities, CancellationToken cancellationToken = default) where T : class;


        // Transaction methods
        Task BeginTransactionAsync();
        Task<int> CommitAsync();
        Task RollbackAsync();
    }
}
