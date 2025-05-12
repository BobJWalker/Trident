using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Facades;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.DataAccess;

namespace Trident.Web.HostedServices
{
    public class SyncJobHostedService : BackgroundService, IDisposable
    {
        private readonly ILogger<SyncJobHostedService> _logger;
        private readonly ISyncRepository _syncRepository;
        private readonly ISyncJobCompositeModelFactory _syncJobCompositeModelFactory;
        private readonly ISyncJobFacade _syncJobFacade;        

        public SyncJobHostedService(ILogger<SyncJobHostedService> logger,
            ISyncRepository syncRepository,
            ISyncJobCompositeModelFactory syncJobCompositeModelFactory,
            ISyncJobFacade syncJobFacade) : base()
        {
            _logger = logger;
            _syncRepository = syncRepository;
            _syncJobCompositeModelFactory = syncJobCompositeModelFactory;
            _syncJobFacade = syncJobFacade;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(30)))
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

                await _syncJobFacade.ProcessSyncJob(syncJobCompositeModel, stoppingToken);
            }
        }               
    }
}
