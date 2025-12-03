using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Base.Common
{
    public class EntityOrderingService(IUnitOfWork unitOfWork, ILogger<EntityOrderingService> logger) : IEntityOrderingService
    {
        public async Task ReorderAfterInsertAsync<T>(string entityId, int newOrder, string orderColumnName = "DisplayOrder") where T : class
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();
                var repository = unitOfWork.GetRepository<T>();
                var queryable = repository.AsQueryable();

                // Dồn các items từ vị trí newOrder trở đi xuống +1
                await queryable
                    .Where(e => EF.Property<int>(e, orderColumnName) >= newOrder)
                    .ExecuteUpdateAsync(update =>
                        update.SetProperty(
                            e => EF.Property<int>(e, orderColumnName),
                            e => EF.Property<int>(e, orderColumnName) + 1));

                await unitOfWork.CommitAsync();
                logger.LogInformation($"Successfully processed reorder after insert for {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(ex, $"Error processing reorder after insert for {typeof(T).Name}");
                throw;
            }
        }

        public async Task ReorderAfterDeleteAsync<T>(int deletedOrder, string orderColumnName = "DisplayOrder") where T : class
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();
                var repository = unitOfWork.GetRepository<T>();
                var queryable = repository.AsQueryable();

                // Dồn các items sau vị trí đã xóa lên -1
                await queryable
                    .Where(e => EF.Property<int>(e, orderColumnName) > deletedOrder)
                    .ExecuteUpdateAsync(update =>
                        update.SetProperty(
                            e => EF.Property<int>(e, orderColumnName),
                            e => EF.Property<int>(e, orderColumnName) - 1));

                await unitOfWork.CommitAsync();
                logger.LogInformation($"Successfully processed reorder after delete for {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(ex, $"Error processing reorder after delete for {typeof(T).Name}");
                throw;
            }
        }

        public async Task ProcessValidateAndFixOrderAsync<T>(string orderColumnName) where T : class
        {
            try
            {
                logger.LogInformation($"Processing validate & fix for {typeof(T).Name}");
                await unitOfWork.BeginTransactionAsync();

                var repo = unitOfWork.GetRepository<T>();
                var q = repo.AsQueryable();

                // Prefetch IDs theo thứ tự hiện tại
                var idProp = typeof(T).GetProperty("Id")
                            ?? throw new InvalidOperationException($"Type {typeof(T).Name} must have Id");
                var ids = await q
                    .OrderBy(e => EF.Property<int>(e, orderColumnName))
                    .Select(e => EF.Property<string>(e, "Id"))
                    .ToListAsync();

                const int batchSize = 1000;
                for (int ofs = 0; ofs < ids.Count; ofs += batchSize)
                {
                    var chunkIds = ids.Skip(ofs).Take(batchSize).ToList();

                    // Load chunk thực thể theo Id
                    var chunk = await repo.AsQueryable()
                        .Where(e => chunkIds.Contains(EF.Property<string>(e, "Id")))
                        .ToListAsync();

                    // Gán DisplayOrder = index toàn cục
                    var indexMap = chunkIds.Select((id, i) => (id, order: ofs + i + 1))
                                           .ToDictionary(x => x.id, x => x.order);

                    foreach (var e in chunk)
                    {
                        var id = (string)idProp.GetValue(e)!;
                        SetOrderValue(e, orderColumnName, indexMap[id]);
                    }

                    repo.UpdateRange(chunk);
                    await unitOfWork.SaveChangesAsync();
                }

                await unitOfWork.CommitAsync();
                logger.LogInformation($"Validate & fix done for {typeof(T).Name}. Total: {ids.Count}");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(ex, $"Error validate & fix {typeof(T).Name}");
                throw;
            }
        }

        public async Task ProcessBatchReorderAsync<T>(List<(string EntityId, int NewOrder)> reorderItems, string orderColumnName) where T : class
        {
            try
            {
                logger.LogInformation($"Processing batch reorder for {typeof(T).Name}. Items count: {reorderItems.Count}");

                var processedCount = 0;
                var failedCount = 0;

                foreach (var item in reorderItems)
                {
                    try
                    {
                        await ReorderAfterInsertAsync<T>(item.EntityId, item.NewOrder, orderColumnName);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to reorder entity {item.EntityId} to position {item.NewOrder}");
                        failedCount++;
                    }
                }

                logger.LogInformation($"Batch reorder completed. Processed: {processedCount}, Failed: {failedCount}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in batch reorder for {typeof(T).Name}");
                throw;
            }
        }

        private static void SetOrderValue(object entity, string orderColumnName, int value)
        {
            var property = entity.GetType().GetProperty(orderColumnName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(entity, value);
            }
        }
    }
}