using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Extensions;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.CompositeModels;
using Trident.Web.DataAccess;

namespace Trident.Web.BusinessLogic.Syncers
{
    public interface IInstanceSyncer
    {
        Task ProcessSyncJob(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken);
    }

    public class InstanceSyncer(
        ILogger<InstanceSyncer> logger,        
        IOctopusRepository octopusRepository,
        IGenericRepository genericRepository,
        ISyncRepository syncRepository,
        ISyncLogModelFactory syncLogModelFactory,
        IEnvironmentSyncer environmentSyncer,
        IProjectSyncer projectSyncer,
        ITenantSyncer tenantSyncer,
        IEventSyncer eventSyncer) : IInstanceSyncer
    {
        public async Task ProcessSyncJob(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken)
        {
            try
            {
                await LogInformation("Setting the starting date to UTC Now", syncJobCompositeModel);
                syncJobCompositeModel.SyncModel.Started = DateTime.UtcNow;
                syncJobCompositeModel.SyncModel.State = SyncState.Started;
                await syncRepository.UpdateAsync(syncJobCompositeModel.SyncModel);

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
            await syncRepository.UpdateAsync(syncJobCompositeModel.SyncModel);
        }

        private async Task<bool> SyncAllRecords(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken)
        {
            try
            {
                await LogInformation($"Getting all the spaces for {syncJobCompositeModel.InstanceModel.Name}", syncJobCompositeModel);
                var octopusSpaces = await octopusRepository.GetAllSpacesAsync(syncJobCompositeModel.InstanceModel);
                await LogInformation($"{octopusSpaces.Count} space(s) found", syncJobCompositeModel);

                foreach (var item in octopusSpaces)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await LogInformation($"Checking to see if space {item.OctopusId}:{item.Name} already exists", syncJobCompositeModel);
                    var spaceModel = await genericRepository.GetByOctopusIdAsync<SpaceModel>(item.OctopusId);
                    await LogInformation($"{(spaceModel != null ? "Space already exists, updating" : "Unable to find space, creating")}", syncJobCompositeModel);
                    item.Id = spaceModel?.Id ?? 0;
                    item.InstanceId = syncJobCompositeModel.InstanceModel.Id;

                    await LogInformation($"Saving space {item.OctopusId}:{item.Name} to the database", syncJobCompositeModel);
                    var spaceToTrack = item.Id > 0 ? await genericRepository.UpdateAsync(item) : await genericRepository.InsertAsync(item);
                    syncJobCompositeModel.SpaceDictionary.Add(item.OctopusId, spaceToTrack);

                    var environmentList = await environmentSyncer.ProcessEnvironments(syncJobCompositeModel, spaceToTrack, stoppingToken);
                    environmentList.ToList().ForEach(x => syncJobCompositeModel.EnvironmentDictionary.Add(x.Key, x.Value));                    

                    var tenantList = await tenantSyncer.ProcessTenants(syncJobCompositeModel, spaceToTrack, stoppingToken);
                    tenantList.ToList().ForEach(x => syncJobCompositeModel.TenantDictionary.Add(x.Key, x.Value));
                    
                    var projectList = await projectSyncer.ProcessProjects(syncJobCompositeModel, spaceToTrack, stoppingToken);
                    projectList.ToList().ForEach(x => syncJobCompositeModel.ProjectDictionary.Add(x.Key, x.Value));                    
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    return true;
                }

                if(syncJobCompositeModel.SyncModel.SearchStartDate.HasValue == true)
                {
                    await eventSyncer.ProcessDeploymentsSinceLastSync(syncJobCompositeModel, stoppingToken);                
                }

                return true;
            }
            catch (Exception ex)
            {
                await LogException($"Exception when processing Sync Job {syncJobCompositeModel.SyncModel.Id} {ex.Message}", syncJobCompositeModel);

                return false;
            }
        }        

        private async Task LogInformation(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{syncJobCompositeModel.GetMessagePrefix()}{message}";
            logger.LogInformation(formattedMessage);
            await genericRepository.InsertAsync(syncLogModelFactory.MakeInformationLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }

        private async Task LogException(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{syncJobCompositeModel.GetMessagePrefix()}{message}";
            logger.LogError(formattedMessage);
            await genericRepository.InsertAsync(syncLogModelFactory.MakeErrorLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }
    }
}
