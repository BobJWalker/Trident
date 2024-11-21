using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models.CompositeModels;
using Trident.Web.DataAccess;
using Trident.Web.BusinessLogic.Syncers;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.OctopusServerModels;

namespace Trident.Web.Tests.BusinessLogic.Syncers
{
    [TestFixture]
    public class EventSyncerTest
    {
        private Mock<ILogger<EventSyncer>> _loggerMock;
        private Mock<ISyncLogRepository> _syncLogRepositoryMock;
        private Mock<IOctopusRepository> _octopusRepositoryMock;
        private Mock<IReleaseRepository> _releaseRepositoryMock;
        private Mock<IDeploymentRepository> _deploymentRepositoryMock;
        private Mock<ISyncLogModelFactory> _syncLogModelFactoryMock;
        private EventSyncer _eventSyncer;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EventSyncer>>();
            _syncLogRepositoryMock = new Mock<ISyncLogRepository>();
            _octopusRepositoryMock = new Mock<IOctopusRepository>();
            _releaseRepositoryMock = new Mock<IReleaseRepository>();
            _deploymentRepositoryMock = new Mock<IDeploymentRepository>();
            _syncLogModelFactoryMock = new Mock<ISyncLogModelFactory>();

            _eventSyncer = new EventSyncer(
                _loggerMock.Object,
                _syncLogRepositoryMock.Object,
                _octopusRepositoryMock.Object,
                _releaseRepositoryMock.Object,
                _deploymentRepositoryMock.Object,
                _syncLogModelFactoryMock.Object
            );
        }

        [Test]
        public async Task ProcessDeploymentsSinceLastSync_ShouldLogInformation()
        {
            // Arrange
            var syncJobCompositeModel = new SyncJobCompositeModel
            {
                SyncModel = new SyncModel { SearchStartDate = DateTime.Now },
                InstanceModel = new InstanceModel(),
                SpaceDictionary = new Dictionary<string, SpaceModel>(),
                ProjectDictionary = new Dictionary<string, ProjectModel>(),
                EnvironmentDictionary = new Dictionary<string, EnvironmentModel>(),
                TenantDictionary = new Dictionary<string, TenantModel>()
            };
            var stoppingToken = new CancellationToken();

            _octopusRepositoryMock.Setup(repo => repo.GetAllEvents(It.IsAny<InstanceModel>(), It.IsAny<SyncModel>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedOctopusModel<EventOctopusModel> { Items = new List<EventOctopusModel>() });

            // Act
            await _eventSyncer.ProcessDeploymentsSinceLastSync(syncJobCompositeModel, stoppingToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
            _syncLogRepositoryMock.Verify(repo => repo.InsertAsync(It.IsAny<SyncLogModel>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task ProcessDeploymentsSinceLastSync_ShouldStopWhenCancellationRequested()
        {
            // Arrange
            var syncJobCompositeModel = new SyncJobCompositeModel
            {
                SyncModel = new SyncModel { SearchStartDate = DateTime.Now },
                InstanceModel = new InstanceModel(),
                SpaceDictionary = new Dictionary<string, SpaceModel>(),
                ProjectDictionary = new Dictionary<string, ProjectModel>(),
                EnvironmentDictionary = new Dictionary<string, EnvironmentModel>(),
                TenantDictionary = new Dictionary<string, TenantModel>()
            };
            var stoppingTokenSource = new CancellationTokenSource();
            stoppingTokenSource.Cancel();

            // Act
            await _eventSyncer.ProcessDeploymentsSinceLastSync(syncJobCompositeModel, stoppingTokenSource.Token);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}