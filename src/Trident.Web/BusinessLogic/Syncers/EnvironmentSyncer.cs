using System;
using System.Collections.Generic;
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
    public interface IEnvironmentSyncer
    {
        Task<Dictionary<string, EnvironmentModel>> ProcessEnvironments(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken);
    }

    public class EnvironmentSyncer : IEnvironmentSyncer
    {
        private readonly ILogger<EnvironmentSyncer> _logger;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly IOctopusRepository _octopusRepository;        
        private readonly IEnvironmentRepository _environmentRepository;
        private readonly ISyncLogModelFactory _syncLogModelFactory;

        public EnvironmentSyncer(
            ILogger<EnvironmentSyncer> logger,
            ISyncLogRepository syncLogRepository,
            IOctopusRepository octopusRepository,            
            IEnvironmentRepository environmentRepository,
            ISyncLogModelFactory syncLogModelFactory)
        {
            _logger = logger;
            _syncLogRepository = syncLogRepository;
            _octopusRepository = octopusRepository;
            _environmentRepository = environmentRepository;
            _syncLogModelFactory = syncLogModelFactory;
        }

        public async Task<Dictionary<string, EnvironmentModel>> ProcessEnvironments(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken)
        {
            await LogInformation($"Getting all the environments for {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);
            var octopusList = await _octopusRepository.GetAllEnvironmentsForSpaceAsync(syncJobCompositeModel.InstanceModel, space);
            await LogInformation($"{octopusList.Count} environments(s) found in {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);

            var returnDictionary = new Dictionary<string, EnvironmentModel>();
            foreach (var item in octopusList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Checking to see if environment {item.OctopusId}:{item.Name} already exists", syncJobCompositeModel);
                var itemModel = await _environmentRepository.GetByOctopusIdAsync(item.OctopusId, space.Id);
                await LogInformation($"{(itemModel != null ? "Environment already exists, updating" : "Unable to find environment, creating")}", syncJobCompositeModel);
                item.Id = itemModel?.Id ?? 0;

                await LogInformation($"Saving environment {item.OctopusId}:{item.Name} to the database", syncJobCompositeModel);
                var modelToTrack = item.Id > 0 ? await _environmentRepository.UpdateAsync(item) : await _environmentRepository.InsertAsync(item);

                await LogInformation($"Adding environment {item.OctopusId}:{item.Name} to our sync dictionary for faster lookup", syncJobCompositeModel);
                returnDictionary.Add(item.OctopusId, modelToTrack);
            }

            return returnDictionary;
        }        

        private async Task LogInformation(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{syncJobCompositeModel.GetMessagePrefix()}{message}";
            _logger.LogInformation(formattedMessage);
            await _syncLogRepository.InsertAsync(_syncLogModelFactory.MakeInformationLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }
    }
}
