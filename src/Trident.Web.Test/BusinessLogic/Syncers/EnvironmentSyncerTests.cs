using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Moq;
using Microsoft.Extensions.Logging;
using Trident.Web.DataAccess;
using Trident.Web.BusinessLogic.Syncers;
using Trident.Web.Core.Models.CompositeModels;

namespace Trident.Web.Test.BusinessLogic.Syncers
{
    [TestFixture]
    public class EnvironmentSyncerTest
    {
        private Mock<ILogger<EnvironmentSyncer>> _loggerMock;
        private Mock<ISyncLogRepository> _syncLogRepositoryMock;
        private Mock<IOctopusRepository> _octopusRepositoryMock;
        private Mock<IEnvironmentRepository> _environmentRepositoryMock;
        private Mock<ISyncLogModelFactory> _syncLogModelFactoryMock;
        private EnvironmentSyncer _environmentSyncer;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EnvironmentSyncer>>();
            _syncLogRepositoryMock = new Mock<ISyncLogRepository>();
            _octopusRepositoryMock = new Mock<IOctopusRepository>();
            _environmentRepositoryMock = new Mock<IEnvironmentRepository>();
            _syncLogModelFactoryMock = new Mock<ISyncLogModelFactory>();

            _environmentSyncer = new EnvironmentSyncer(
                _loggerMock.Object,
                _syncLogRepositoryMock.Object,
                _octopusRepositoryMock.Object,
                _environmentRepositoryMock.Object,
                _syncLogModelFactoryMock.Object);
        }

        [Test]
        public async Task ProcessEnvironments_ShouldReturnDictionary_WhenEnvironmentsExist()
        {
            // Arrange
            var syncJobCompositeModel = new SyncJobCompositeModel
            {
                InstanceModel = new InstanceModel { Name = "Instance1" },
                SyncModel = new SyncModel { Id = 1 }
            };
            var space = new SpaceModel { Name = "Space1", Id = 1 };
            var stoppingToken = CancellationToken.None;

            var octopusEnvironments = new List<EnvironmentModel>
            {
                new EnvironmentModel { OctopusId = "1", Name = "Env1" },
                new EnvironmentModel { OctopusId = "2", Name = "Env2" }
            };

            _octopusRepositoryMock.Setup(repo => repo.GetAllEnvironmentsForSpaceAsync(syncJobCompositeModel.InstanceModel, space))
                .ReturnsAsync(octopusEnvironments);

            _environmentRepositoryMock.Setup(repo => repo.GetByOctopusIdAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((EnvironmentModel)null);

            _environmentRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<EnvironmentModel>()))
                .ReturnsAsync((EnvironmentModel env) => env);

            // Act
            var result = await _environmentSyncer.ProcessEnvironments(syncJobCompositeModel, space, stoppingToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Keys, Does.Contain("1"));
            Assert.That(result.Keys, Does.Contain("2"));
        }

        [Test]
        public async Task ProcessEnvironments_ShouldUpdateExistingEnvironment_WhenEnvironmentExists()
        {
            // Arrange
            var syncJobCompositeModel = new SyncJobCompositeModel
            {
                InstanceModel = new InstanceModel { Name = "Instance1" },
                SyncModel = new SyncModel { Id = 1 }
            };
            var space = new SpaceModel { Name = "Space1", Id = 1 };
            var stoppingToken = CancellationToken.None;

            var octopusEnvironments = new List<EnvironmentModel>
            {
                new EnvironmentModel { OctopusId = "1", Name = "Env1" }
            };

            var existingEnvironment = new EnvironmentModel { Id = 1, OctopusId = "1", Name = "Env1" };

            _octopusRepositoryMock.Setup(repo => repo.GetAllEnvironmentsForSpaceAsync(syncJobCompositeModel.InstanceModel, space))
                .ReturnsAsync(octopusEnvironments);

            _environmentRepositoryMock.Setup(repo => repo.GetByOctopusIdAsync("1", space.Id))
                .ReturnsAsync(existingEnvironment);

            _environmentRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<EnvironmentModel>()))
                .ReturnsAsync((EnvironmentModel env) => env);

            // Act
            var result = await _environmentSyncer.ProcessEnvironments(syncJobCompositeModel, space, stoppingToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Keys, Does.Contain("1"));
            Assert.That(result["1"].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task ProcessEnvironments_ShouldHandleCancellation()
        {
            // Arrange
            var syncJobCompositeModel = new SyncJobCompositeModel
            {
                InstanceModel = new InstanceModel { Name = "Instance1" },
                SyncModel = new SyncModel { Id = 1 }
            };
            var space = new SpaceModel { Name = "Space1", Id = 1 };
            var stoppingTokenSource = new CancellationTokenSource();
            var stoppingToken = stoppingTokenSource.Token;

            var octopusEnvironments = new List<EnvironmentModel>
            {
                new EnvironmentModel { OctopusId = "1", Name = "Env1" },
                new EnvironmentModel { OctopusId = "2", Name = "Env2" }
            };

            _octopusRepositoryMock.Setup(repo => repo.GetAllEnvironmentsForSpaceAsync(syncJobCompositeModel.InstanceModel, space))
                .ReturnsAsync(octopusEnvironments);

            stoppingTokenSource.Cancel();

            // Act
            var result = await _environmentSyncer.ProcessEnvironments(syncJobCompositeModel, space, stoppingToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}