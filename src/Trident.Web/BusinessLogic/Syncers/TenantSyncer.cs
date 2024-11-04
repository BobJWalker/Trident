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
    public interface ITenantSyncer
    {
        Task<Dictionary<string, TenantModel>> ProcessTenants(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken);
    }

    public class TenantSyncer : ITenantSyncer
    {
        private readonly ILogger<TenantSyncer> _logger;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly IOctopusRepository _octopusRepository;        
        private readonly ITenantRepository _tenantRepository;
        private readonly ISyncLogModelFactory _syncLogModelFactory;

        public TenantSyncer(
            ILogger<TenantSyncer> logger,
            ISyncLogRepository syncLogRepository,
            IOctopusRepository octopusRepository,            
            ITenantRepository tenantRepository,
            ISyncLogModelFactory syncLogModelFactory)
        {
            _logger = logger;
            _syncLogRepository = syncLogRepository;
            _octopusRepository = octopusRepository;
            _tenantRepository = tenantRepository;
            _syncLogModelFactory = syncLogModelFactory;
        }

        public async Task<Dictionary<string, TenantModel>> ProcessTenants(SyncJobCompositeModel syncJobCompositeModel, SpaceModel space, CancellationToken stoppingToken)
        {
            await LogInformation($"Getting all the tenants for {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);
            var octopusList = await _octopusRepository.GetAllTenantsForSpaceAsync(syncJobCompositeModel.InstanceModel, space);
            await LogInformation($"{octopusList.Count} tenants(s) found in {syncJobCompositeModel.InstanceModel.Name}:{space.Name}", syncJobCompositeModel);

            var returnObject = new Dictionary<string, TenantModel>();
            foreach (var item in octopusList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await LogInformation($"Checking to see if tenant {item.OctopusId}:{item.Name} already exists", syncJobCompositeModel);
                var itemModel = await _tenantRepository.GetByOctopusIdAsync(item.OctopusId, space.Id);
                await LogInformation($"{(itemModel != null ? "Tenant already exists, updating" : "Unable to find tenant, creating")}", syncJobCompositeModel);
                item.Id = itemModel?.Id ?? 0;

                await LogInformation($"Saving tenant {item.OctopusId}:{item.Name} to the database", syncJobCompositeModel);
                var modelToTrack = item.Id > 0 ? await _tenantRepository.UpdateAsync(item) : await _tenantRepository.InsertAsync(item);

                await LogInformation($"Adding tenant {item.OctopusId}:{item.Name} to our sync dictionary for faster lookup", syncJobCompositeModel);
                returnObject.Add(item.OctopusId, modelToTrack);
            }

            return returnObject;
        }       

        private async Task LogInformation(string message, SyncJobCompositeModel syncJobCompositeModel)
        {
            var formattedMessage = $"{syncJobCompositeModel.GetMessagePrefix()}{message}";
            _logger.LogInformation(formattedMessage);
            await _syncLogRepository.InsertAsync(_syncLogModelFactory.MakeInformationLog(formattedMessage, syncJobCompositeModel.SyncModel.Id));
        }
    }
}
