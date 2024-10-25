using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.BusinessLogic.Syncers;
using Trident.Web.DataAccess;

namespace Trident.Web.HostedServices
{
    public class SyncJobHostedService : BackgroundService, IDisposable
    {
        private readonly ILogger<SyncJobHostedService> _logger;
        private readonly ISyncRepository _syncRepository;
        private readonly ISyncJobCompositeModelFactory _syncJobCompositeModelFactory;
        private readonly IInstanceSyncer _instanceSyncer;        

        public SyncJobHostedService(ILogger<SyncJobHostedService> logger,
            ISyncRepository syncRepository,
            ISyncJobCompositeModelFactory syncJobCompositeModelFactory,
            IInstanceSyncer instanceSyncer) : base()
        {
            _logger = logger;
            _syncRepository = syncRepository;
            _syncJobCompositeModelFactory = syncJobCompositeModelFactory;
            _instanceSyncer = instanceSyncer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            var recordsToProcess = await _syncRepository.GetNextRecordsToProcessAsync();

            _logger.LogInformation($"Found {recordsToProcess.Items.Count} record(s) to process.");

            foreach (var syncJob in recordsToProcess.Items)
            {
                var syncJobCompositeModel = await _syncJobCompositeModelFactory.MakeSyncJobCompositeModelAsync(syncJob);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await _instanceSyncer.ProcessSyncJob(syncJobCompositeModel, stoppingToken);
            }
        }               
    }
}
