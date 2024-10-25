using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models.CompositeModels;
using Trident.Web.DataAccess;

namespace Trident.Web.BusinessLogic.Syncers
{
    public interface IInstanceSyncer
    {
        Task ProcessSyncJob(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken);
    }

    public class InstanceSyncer : IInstanceSyncer
    {
        private readonly ILogger<InstanceSyncer> _logger;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly IOctopusRepository _octopusRepository;
        private readonly ISpaceRepository _spaceRepository;
        private readonly ISyncRepository _syncRepository;
        private readonly ISyncLogModelFactory _syncLogModelFactory;
        private readonly IEnvironmentSyncer _environmentSyncer;
        private readonly IProjectSyncer _projectSyncer;
        private readonly ITenantSyncer _tenantSyncer;
        private readonly IEventSyncer _eventSyncer;

        public InstanceSyncer(
            ILogger<InstanceSyncer> logger,
            ISyncLogRepository syncLogRepository,
            IOctopusRepository octopusRepository,
            ISpaceRepository spaceRepository,
            ISyncRepository syncRepository,
            ISyncLogModelFactory syncLogModelFactory,
            IEnvironmentSyncer environmentSyncer,
            IProjectSyncer projectSyncer,
            ITenantSyncer tenantSyncer,
            IEventSyncer eventSyncer)
        {
            _logger = logger;
            _syncLogRepository = syncLogRepository;
            _octopusRepository = octopusRepository;
            _spaceRepository = spaceRepository;
            _syncRepository = syncRepository;
            _syncLogModelFactory = syncLogModelFactory;
            _environmentSyncer = environmentSyncer;
            _projectSyncer = projectSyncer;
            _tenantSyncer = tenantSyncer;
            _eventSyncer = eventSyncer;
        }

        public async Task ProcessSyncJob(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken)
        {
            try
            {
                await LogInformation("Setting the starting date to UTC Now", syncJobCompositeModel);
                syncJobCompositeModel.SyncModel.Started = DateTime.UtcNow;
                syncJobCompositeModel.SyncModel.State = SyncState.Started;
                await _syncRepository.UpdateAsync(syncJobCompositeModel.SyncModel);

                var isSuccessful = await SyncAllRecords(syncJobCompositeModel, stoppingToken);

                await ProcessStatus(syncJobCompositeModel, isSuccessful);
            }
            catch (Exception ex)
            {
                await LogException($"Exception when processing Sync Job {syncJobCompositeModel.SyncModel.Id} {ex.Message}", syncJobCompositeModel);

                await ProcessStatus(syncJobCompositeModel, isSuccessful: false);
            }
        }

        private async Task ProcessStatus(SyncJobCompositeModel syncJobCompositeModel, bool isSuccessful)
        {
            if (isSuccessful)
            {
                await LogInformation("The sync was successful, setting the completed date to UTC Now and the state to completed", syncJobCompositeModel);
                syncJobCompositeModel.SyncModel.Completed = DateTime.UtcNow;
                syncJobCompositeModel.SyncModel.State = SyncState.Completed;
            }
            else
            {
                await LogInformation("The sync failed, incrementing retry counter", syncJobCompositeModel);
                syncJobCompositeModel.SyncModel.RetryAttempts = syncJobCompositeModel.SyncModel.RetryAttempts.GetValueOrDefault() + 1;

                if (syncJobCompositeModel.SyncModel.RetryAttempts >= 5)
                {
                    await LogInformation("The retry counter has gone over 5, failing this sync", syncJobCompositeModel);
                    syncJobCompositeModel.SyncModel.State = SyncState.Failed;
                }
                else
                {
                    await LogInformation("Retry attempts are still possible, setting the start date to null", syncJobCompositeModel);
                    syncJobCompositeModel.SyncModel.Started = null;
                    syncJobCompositeModel.SyncModel.State = SyncState.Queued;
                }
            }

            await LogInformation("Updating the sync job", syncJobCompositeModel);
            await _syncRepository.UpdateAsync(syncJobCompositeModel.SyncModel);
        }

        private async Task<bool> SyncAllRecords(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken)
        {
            try
            {
                await LogInformation($"Getting all the spaces for {syncJobCompositeModel.InstanceModel.Name}", syncJobCompositeModel);
                var octopusSpaces = await _octopusRepository.GetAllSpacesAsync(syncJobCompositeModel.InstanceModel);
                await LogInformation($"{octopusSpaces.Count} space(s) found", syncJobCompositeModel);

                foreach (var item in octopusSpaces)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await LogInformation($"Checking to see if space {item.OctopusId}:{item.Name} already exists", syncJobCompositeModel);
                    var spaceModel = await _spaceRepository.GetByOctopusIdAsync(item.OctopusId, syncJobCompositeModel.InstanceModel.Id);
                    await LogInformation($"{(spaceModel != null ? "Space already exists, updating" : "Unable to find space, creating")}", syncJobCompositeModel);
                    item.Id = spaceModel?.Id ?? 0;
                    item.InstanceId = syncJobCompositeModel.InstanceModel.Id;

                    await LogInformation($"Saving space {item.OctopusId}:{item.Name} to the database", syncJobCompositeModel);
                    var spaceToTrack = item.Id > 0 ? await _spaceRepository.UpdateAsync(item) : await _spaceRepository.InsertAsync(item);

                    syncJobCompositeModel.SpaceDictionary.Add(item.OctopusId, spaceToTrack);

                    await _environmentSyncer.ProcessEnvironments(syncJobCompositeModel, spaceToTrack, stoppingToken);
                    await _tenantSyncer.ProcessTenants(syncJobCompositeModel, spaceToTrack, stoppingToken);
                    await _projectSyncer.ProcessProjects(syncJobCompositeModel, spaceToTrack, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    return true;
                }

                if(syncJobCompositeModel.SyncModel.SearchStartDate.HasValue == true)
                {
                    await _eventSyncer.ProcessDeploymentsSinceLastSync(syncJobCompositeModel, stoppingToken);                
                }

                return true;
            }
            catch (Exception ex)
            {
                await LogException($"Exception when processing Sync Job {syncJobCompositeModel.SyncModel.Id} {ex.Message}", syncJobCompositeModel);

                return false;
            }
        }

        private string GetMessagePrefix(SyncJobCompositeModel syncJobCompositeModel)
        {
            return $"Sync {syncJobCompositeModel.SyncModel.Id}: ";
        }

        private async Task LogInformation(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{GetMessagePrefix(syncJobCompositeModel)}{message}";
            _logger.LogInformation(formattedMessage);
            await _syncLogRepository.InsertAsync(_syncLogModelFactory.MakeInformationLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }

        private async Task LogException(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{GetMessagePrefix(syncJobCompositeModel)}{message}";
            _logger.LogError(formattedMessage);
            await _syncLogRepository.InsertAsync(_syncLogModelFactory.MakeErrorLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }
    }
}
