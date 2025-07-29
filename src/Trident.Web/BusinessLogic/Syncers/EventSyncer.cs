using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Extensions;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.CompositeModels;
using Trident.Web.DataAccess;

namespace Trident.Web.BusinessLogic.Syncers
{
    public interface IEventSyncer
    {
         Task ProcessDeploymentsSinceLastSync(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken);
    }

    public class EventSyncer : IEventSyncer
    {
        private readonly ILogger<EventSyncer> _logger;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly IOctopusRepository _octopusRepository;        
        private readonly IGenericRepository<ReleaseModel> _releaseRepository;
        private readonly IGenericRepository<DeploymentModel> _deploymentRepository;        
        private readonly ISyncLogModelFactory _syncLogModelFactory;

        public EventSyncer(
            ILogger<EventSyncer> logger,
            ISyncLogRepository syncLogRepository,
            IOctopusRepository octopusRepository,            
            IGenericRepository<ReleaseModel> releaseRepository,
            IGenericRepository<DeploymentModel> deploymentRepository,            
            ISyncLogModelFactory syncLogModelFactory)
        {
            _logger = logger;
            _syncLogRepository = syncLogRepository;
            _octopusRepository = octopusRepository;            
            _releaseRepository = releaseRepository;
            _deploymentRepository = deploymentRepository;            
            _syncLogModelFactory = syncLogModelFactory;
        }

        public async Task ProcessDeploymentsSinceLastSync(SyncJobCompositeModel syncJobCompositeModel, CancellationToken stoppingToken)
        {
            var startIndex = 0;
            var canContinue = true;

            await LogInformation($"Finding all deployments since {syncJobCompositeModel.SyncModel.SearchStartDate}", syncJobCompositeModel);            

            while (canContinue)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Getting the next results at {startIndex}", syncJobCompositeModel);
                var eventResults = await _octopusRepository.GetAllEvents(syncJobCompositeModel.InstanceModel, syncJobCompositeModel.SyncModel, startIndex);

                foreach (var octopusEvent in eventResults.Items)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var spaceId = octopusEvent.SpaceId;
                    var space = syncJobCompositeModel.SpaceDictionary[spaceId];

                    var projectId = octopusEvent.RelatedDocumentIds.First(x => x.StartsWith("Projects"));
                    var project = syncJobCompositeModel.ProjectDictionary[projectId];

                    var releaseId = octopusEvent.RelatedDocumentIds.First(x => x.StartsWith("Release"));                    
                    var releaseModelToTrack = await _releaseRepository.GetByIdAsync(int.Parse(releaseId));

                        var deploymentId = octopusEvent.RelatedDocumentIds.First(x => x.StartsWith("DeploymentId"));
                        var deploymentModel = await _octopusRepository.GetSpecificDeployment(syncJobCompositeModel.InstanceModel, space, releaseModelToTrack, deploymentId, syncJobCompositeModel.EnvironmentDictionary, syncJobCompositeModel.TenantDictionary);

                        if (deploymentModel != null)
                        {
                            var itemModel = await _deploymentRepository.GetByIdAsync(deploymentModel.Id);
                            await LogInformation($"{(itemModel != null ? "Deployment already exists, updating" : "Unable to find deployment, creating")}", syncJobCompositeModel);
                            deploymentModel.Id = itemModel?.Id ?? 0;

                            await LogInformation($"Saving deployment {deploymentModel.OctopusId} to the database", syncJobCompositeModel);
                            var modelToTrack = deploymentModel.Id > 0 ? await _deploymentRepository.UpdateAsync(deploymentModel) : await _deploymentRepository.InsertAsync(deploymentModel);
                        }
                }

                canContinue = eventResults.Items.Count > 0;
                startIndex += 10;
            }
            
        }

        private async Task LogInformation(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{syncJobCompositeModel.GetMessagePrefix()}{message}";
            _logger.LogInformation(formattedMessage);
            await _syncLogRepository.InsertAsync(_syncLogModelFactory.MakeInformationLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }
    }
}
