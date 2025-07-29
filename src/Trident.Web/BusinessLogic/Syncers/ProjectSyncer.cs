using System.Collections.Generic;
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
    public interface IProjectSyncer
    {
         Task<Dictionary<string, ProjectModel>> ProcessProjects(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken);
    }

    public class ProjectSyncer : IProjectSyncer
    {
        private readonly ILogger<ProjectSyncer> _logger;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly IOctopusRepository _octopusRepository;        
        private readonly IGenericRepository<ProjectModel> _projectRepository;        
        private readonly IGenericRepository<ReleaseModel> _releaseRepository;
        private readonly IGenericRepository<DeploymentModel> _deploymentRepository;        
        private readonly ISyncLogModelFactory _syncLogModelFactory;

        public ProjectSyncer(
            ILogger<ProjectSyncer> logger,
            ISyncLogRepository syncLogRepository,
            IOctopusRepository octopusRepository,            
            IGenericRepository<ProjectModel> projectRepository,            
            IGenericRepository<ReleaseModel> releaseRepository,
            IGenericRepository<DeploymentModel> deploymentRepository,            
            ISyncLogModelFactory syncLogModelFactory)
        {
            _logger = logger;
            _syncLogRepository = syncLogRepository;
            _octopusRepository = octopusRepository;            
            _projectRepository = projectRepository;            
            _releaseRepository = releaseRepository;
            _deploymentRepository = deploymentRepository;            
            _syncLogModelFactory = syncLogModelFactory;
        }

        public async Task<Dictionary<string, ProjectModel>> ProcessProjects(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken)
        {
            await LogInformation($"Getting all the projects for {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);
            var octopusList = await _octopusRepository.GetAllProjectsForSpaceAsync(syncJobCompositeModel.InstanceModel, space);
            await LogInformation($"{octopusList.Count} projects(s) found in {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);

            var returnObject = new Dictionary<string, ProjectModel>();

            foreach (var item in octopusList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Checking to see if project {item.OctopusId}:{item.Name} already exists", syncJobCompositeModel);
                var itemModel = await _projectRepository.GetByOctopusIdAsync(item.OctopusId);
                await LogInformation($"{(itemModel != null ? "Project already exists, updating" : "Unable to find project, creating")}", syncJobCompositeModel);
                item.Id = itemModel?.Id ?? 0;

                await LogInformation($"Saving project {item.OctopusId}:{item.Name} to the database", syncJobCompositeModel);
                var modelToTrack = item.Id > 0 ? await _projectRepository.UpdateAsync(item) : await _projectRepository.InsertAsync(item);

                returnObject.Add(item.OctopusId, modelToTrack);

                await ProcessReleasesForProject(syncJobCompositeModel, space, item, stoppingToken);
            }

            return returnObject;
        }

        private async Task ProcessReleasesForProject(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, ProjectModel project, CancellationToken stoppingToken)
        {
            await LogInformation($"Getting all the releases for {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}", syncJobCompositeModel);
            var octopusList = await _octopusRepository.GetAllReleasesForProjectAsync(syncJobCompositeModel.InstanceModel, space, project);
            await LogInformation($"{octopusList.Count} releases(s) found in {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}", syncJobCompositeModel);

            foreach (var item in octopusList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Checking to see if release {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}:{item.OctopusId}:{item.Version} already exists", syncJobCompositeModel);
                var itemModel = await _releaseRepository.GetByOctopusIdAsync(item.OctopusId);
                await LogInformation($"{(itemModel != null ? "Release already exists, updating" : "Unable to find release, creating")}", syncJobCompositeModel);
                item.Id = itemModel?.Id ?? 0;

                await LogInformation($"Saving release {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}:{item.OctopusId}:{item.Version} to the database", syncJobCompositeModel);
                var modelToTrack = item.Id > 0 ? await _releaseRepository.UpdateAsync(item) : await _releaseRepository.InsertAsync(item);

                if (syncJobCompositeModel.SyncModel.SearchStartDate.HasValue == false)
                {
                    await ProcessDeploymentsForProjectsRelease(syncJobCompositeModel, space, project, modelToTrack, stoppingToken);
                }
            }
        }

        private async Task ProcessDeploymentsForProjectsRelease(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, ProjectModel project, ReleaseModel releaseModel, CancellationToken stoppingToken)
        {
            await LogInformation($"Getting all the deployments for {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}:{releaseModel.Version}", syncJobCompositeModel);
            var octopusList = await _octopusRepository.GetAllDeploymentsForReleaseAsync(syncJobCompositeModel.InstanceModel, space, project, releaseModel, syncJobCompositeModel.EnvironmentDictionary, syncJobCompositeModel.TenantDictionary);
            await LogInformation($"{octopusList.Count} deployments(s) found in {syncJobCompositeModel.InstanceModel.Name}:{space.Name}:{project.Name}:{releaseModel.Version}", syncJobCompositeModel);

            foreach (var item in octopusList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Checking to see if deployment {item.OctopusId} already exists", syncJobCompositeModel);
                var itemModel = await _deploymentRepository.GetByOctopusIdAsync(item.OctopusId);
                await LogInformation($"{(itemModel != null ? "Deployment already exists, updating" : "Unable to find deployment, creating")}", syncJobCompositeModel);
                item.Id = itemModel?.Id ?? 0;

                await LogInformation($"Saving deployment {item.OctopusId} to the database", syncJobCompositeModel);
                if (item.Id > 0)
                {
                    await _deploymentRepository.UpdateAsync(item);
                }
                else
                {
                    await _deploymentRepository.InsertAsync(item);
                }                
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
