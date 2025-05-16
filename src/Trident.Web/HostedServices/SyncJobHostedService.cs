using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature;
using Trident.Web.BusinessLogic.Facades;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.DataAccess;

namespace Trident.Web.HostedServices
{
    public class SyncJobHostedService(ILogger<SyncJobHostedService> logger,
            ISyncRepository syncRepository,
            ISyncJobCompositeModelFactory syncJobCompositeModelFactory,
            ISyncJobFacade syncJobFacade,
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
            var recordsToProcess = await syncRepository.GetNextRecordsToProcessAsync();

            logger.LogInformation($"Found {recordsToProcess.Items.Count} record(s) to process.");

            foreach (var syncJob in recordsToProcess.Items)
            {
                var syncJobCompositeModel = await syncJobCompositeModelFactory.MakeSyncJobCompositeModelAsync(syncJob);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await syncJobFacade.ProcessSyncJob(syncJobCompositeModel, stoppingToken);
            }
        }               
    }
}
