using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.BusinessLogic.Syncers;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.HostedServices
{
    public class SyncJobHostedService(ILogger<SyncJobHostedService> logger,
            ITridentDataAdapter tridentDataAdapter,
            ISyncJobCompositeModelFactory syncJobCompositeModelFactory,
            IInstanceSyncer instanceSyncer,
            IFeatureClient featureClient) 
        : BackgroundService, IDisposable
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var isEnabled = await featureClient.GetBooleanValueAsync("BackgroundSyncJob", false);

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

            foreach (var syncJob in recordsToProcess.Items)
            {
                var syncJobCompositeModel = await syncJobCompositeModelFactory.MakeSyncJobCompositeModelAsync(syncJob);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await instanceSyncer.ProcessSyncJob(syncJobCompositeModel, stoppingToken);
            }
        }

        private Task<PagedViewModel<SyncModel>> GetNextRecordsToProcessAsync()
        {
            const int rowsPerPage = 5;
            const int currentPageNumber = 1;
            var whereClause = $"Where State = '{SyncState.Queued}' and IsNull(RetryAttempts,0) < 5";

            return tridentDataAdapter.GetAllAsync<SyncModel>(currentPageNumber, rowsPerPage, "Created", isAsc: true, whereClause);
        }
    }
}
