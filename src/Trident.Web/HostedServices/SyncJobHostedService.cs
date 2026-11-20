using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Syncers;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;
using Trident.Web.Core.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.HostedServices
{
    public class SyncJobHostedService(
            ILogger<SyncJobHostedService> logger,
            ITridentDataAdapter tridentDataAdapter,            
            ITridentFeatureFlag tridentFeatureFlag,
            IFeedSyncer feedSyncer) 
        : BackgroundService, IDisposable
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await FailAllProcessingJobsAsync();
            var isEnabled = await tridentFeatureFlag.IsEnabledAsync(TridentFeatureFlagList.BackgroundSyncJob);

            if (isEnabled == false)
            {
                logger.LogInformation("Background sync job is disabled.");
                return;
            }

            using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(15)))
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    while(await timer.WaitForNextTickAsync())
                    {
                        await RunJobAsync(stoppingToken);
                    }
                }
            }
        }

        private async Task RunJobAsync(CancellationToken stoppingToken)
        {
            var recordsToProcess = await GetNextRecordsToProcessAsync();

            logger.LogInformation($"Found {recordsToProcess.Items.Count} record(s) to process.");

            for (int i = 0; i < recordsToProcess.Items.Count; i++)
            {
                var syncJob = recordsToProcess.Items[i];

                syncJob.Started = DateTime.UtcNow;
                syncJob.State = SyncState.Started;
                syncJob.RetryAttempts += 1;

                await tridentDataAdapter.UpdateAsync(syncJob);

                try
                {
                    await feedSyncer.SyncAllFeedsAsync(syncJob);

                    syncJob.Completed = DateTime.UtcNow;
                    syncJob.State = SyncState.Completed;

                    await tridentDataAdapter.UpdateAsync(syncJob);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    if (syncJob.RetryAttempts < 3)
                    {
                        logger.LogWarning("There have been fewer than three errors, reseting the status to Queued");
                        syncJob.State = SyncState.Queued;
                    }
                    else
                    {
                        logger.LogError("There have been moret han three errors, setting status to Failed");
                        syncJob.State = SyncState.Failed;
                    }

                    await tridentDataAdapter.UpdateAsync(syncJob);
                }
            }
        }

        private Task<PagedViewModel<SyncModel>> GetNextRecordsToProcessAsync()
        {
            const int rowsPerPage = 5;
            const int currentPageNumber = 1;
            var whereClause = $"Where State = '{SyncState.Queued}' and IsNull(RetryAttempts,0) < 3";

            return tridentDataAdapter.GetAllAsync<SyncModel>(currentPageNumber, rowsPerPage, "Created", isAsc: true, whereClause);
        }

        private async Task FailAllProcessingJobsAsync()
        {
            const int rowsPerPage = 500;
            const int currentPageNumber = 1;
            var whereClause = $"Where State in '{SyncState.Started}', '{SyncState.Queued}')";

            var result = await tridentDataAdapter.GetAllAsync<SyncModel>(currentPageNumber, rowsPerPage, "Created", isAsc: true, whereClause);

            foreach (var syncJob in result.Items)
            {
                logger.LogWarning($"SyncJob {syncJob.Id} is currently {syncJob.State}, setting it to failed.");

                syncJob.State = SyncState.Failed;
                syncJob.Completed = DateTime.UtcNow;

                await tridentDataAdapter.UpdateAsync(syncJob);
            }
        }
    }
}
