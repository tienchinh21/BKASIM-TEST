namespace MiniAppGIBA.Base.Interface
{
    public interface IEntityOrderingService
    {
        Task ReorderAfterInsertAsync<T>(string entityId, int newOrder, string orderColumnName = "DisplayOrder") where T : class;
        Task ReorderAfterDeleteAsync<T>(int deletedOrder, string orderColumnName = "DisplayOrder") where T : class;
        Task ProcessValidateAndFixOrderAsync<T>(string orderColumnName) where T : class;
        Task ProcessBatchReorderAsync<T>(List<(string EntityId, int NewOrder)> reorderItems, string orderColumnName) where T : class;
    }
}