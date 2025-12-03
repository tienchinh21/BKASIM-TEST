using MiniAppGIBA.Base.Interface;

namespace MiniAppGIBA.Base.Common
{
    /// <summary>
    /// Simple wrapper service for ordering - calls IEntityOrderingService directly (without Hangfire)
    /// </summary>
    public class HangfireOrderingService(IEntityOrderingService entityOrderingService, ILogger<HangfireOrderingService> logger) : IHangfireOrderingService
    {
        public string ScheduleReorderJob<T>(string entityId, int newOrder, string orderColumnName) where T : class
        {
            try
            {
                // Call directly without Hangfire (async fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ReorderAfterInsertAsync<T>(entityId, newOrder, orderColumnName);
                        logger.LogInformation("Completed reorder job for {Type} {Id} -> {Order}", typeof(T).Name, entityId, newOrder);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleReorderJob for {Type} {Id}", typeof(T).Name, entityId);
                    }
                });
                logger.LogInformation("Scheduled reorder job for {Type} {Id} -> {Order}", typeof(T).Name, entityId, newOrder);
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling reorder job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleInsertFirstJob<T>(string entityId, string orderColumnName) where T : class
        {
            try
            {
                // Insert at position 1
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ReorderAfterInsertAsync<T>(entityId, 1, orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleInsertFirstJob for {Type} {Id}", typeof(T).Name, entityId);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling insert first job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleInsertAtPositionJob<T>(string entityId, int position, string orderColumnName) where T : class
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ReorderAfterInsertAsync<T>(entityId, position, orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleInsertAtPositionJob for {Type} {Id}", typeof(T).Name, entityId);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling insert at position job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleReorderAfterDeleteJob<T>(int deletedOrder, string orderColumnName) where T : class
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ReorderAfterDeleteAsync<T>(deletedOrder, orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleReorderAfterDeleteJob for {Type}", typeof(T).Name);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling reorder after delete job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleValidateAndFixJob<T>(string orderColumnName) where T : class
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ProcessValidateAndFixOrderAsync<T>(orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleValidateAndFixJob for {Type}", typeof(T).Name);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling validate and fix job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleBatchReorderJob<T>(List<(string EntityId, int NewOrder)> reorderItems, string orderColumnName) where T : class
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await entityOrderingService.ProcessBatchReorderAsync<T>(reorderItems, orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleBatchReorderJob for {Type}", typeof(T).Name);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling batch reorder job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public string ScheduleDelayedReorderJob<T>(string entityId, int newOrder, string orderColumnName, TimeSpan delay) where T : class
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(delay);
                    try
                    {
                        await entityOrderingService.ReorderAfterInsertAsync<T>(entityId, newOrder, orderColumnName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in ScheduleDelayedReorderJob for {Type} {Id}", typeof(T).Name, entityId);
                    }
                });
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scheduling delayed reorder job for {Type}", typeof(T).Name);
                return string.Empty;
            }
        }

        public void ScheduleRecurringOrderMaintenanceJob<T>(string jobId, string orderColumnName, string cronExpression) where T : class
        {
            // Not implemented - would need Hangfire for recurring jobs
            logger.LogWarning("Recurring jobs not supported without Hangfire. JobId: {JobId}, Type: {Type}", jobId, typeof(T).Name);
        }
    }
}

