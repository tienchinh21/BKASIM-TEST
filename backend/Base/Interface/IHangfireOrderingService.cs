namespace MiniAppGIBA.Base.Interface
{
    public interface IHangfireOrderingService
    {
        /// <summary>
        /// Schedule job sắp xếp entity
        /// </summary>
        string ScheduleReorderJob<T>(string entityId, int newOrder, string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job insert entity mới vào vị trí đầu tiên
        /// </summary>
        string ScheduleInsertFirstJob<T>(string entityId, string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job insert entity mới vào vị trí cụ thể
        /// </summary>
        string ScheduleInsertAtPositionJob<T>(string entityId, int position, string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job sắp xếp lại sau khi xóa
        /// </summary>
        string ScheduleReorderAfterDeleteJob<T>(int deletedOrder, string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job validate và fix thứ tự
        /// </summary>
        string ScheduleValidateAndFixJob<T>(string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job batch reorder
        /// </summary>
        string ScheduleBatchReorderJob<T>(List<(string EntityId, int NewOrder)> reorderItems, string orderColumnName) where T : class;

        /// <summary>
        /// Schedule job với delay
        /// </summary>
        string ScheduleDelayedReorderJob<T>(string entityId, int newOrder, string orderColumnName, TimeSpan delay) where T : class;

        /// <summary>
        /// Schedule recurring job để maintain order integrity
        /// </summary>
        void ScheduleRecurringOrderMaintenanceJob<T>(string jobId, string orderColumnName, string cronExpression) where T : class;
    }
}

