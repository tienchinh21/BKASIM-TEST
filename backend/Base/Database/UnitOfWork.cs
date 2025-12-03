using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MiniAppGIBA.Base.Interface;

namespace MiniAppGIBA.Base.Database
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext context = context;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private IDbContextTransaction _transaction;

        public ApplicationDbContext Context => context;

        public IRepository<T> GetRepository<T>() where T : class
        {
            if (repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)repositories[typeof(T)];
            }
            var repository = new Repository<T>(context);
            repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// Bulk Insert dữ liệu nhanh hơn SaveChangesAsync()
        /// </summary>
        public async Task BulkInsertAsync<T>(IList<T> entities, CancellationToken cancellationToken = default) where T : class
        {
            if (entities == null || entities.Count == 0)
                return;

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                // await context.BulkInsertAsync(entities);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        // Bắt đầu transaction
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await context.Database.BeginTransactionAsync();
            }
        }

        // Xác nhận transaction
        public async Task<int> CommitAsync()
        {
            if (_transaction != null)
            {
                int affectedRows = await context.SaveChangesAsync();
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
                return affectedRows;
            }
            return 0;
        }

        // Hủy transaction nếu có lỗi
        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            context.Dispose();
            _transaction?.Dispose();
        }
    }
}
