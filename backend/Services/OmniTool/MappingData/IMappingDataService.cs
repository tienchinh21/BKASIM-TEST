namespace MiniAppGIBA.Services.OmniTool.MappingData
{
    public interface IMappingDataService
    {
        Task<Dictionary<string, object>?> GetDataAsync(string tableName, string columnName, Dictionary<string, object>? filters = null);
    }
}
