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
        private Mock<IGenericRepository<ReleaseModel>> _releaseRepositoryMock;
        private Mock<IGenericRepository<DeploymentModel>> _deploymentRepositoryMock;
        private Mock<ISyncLogModelFactory> _syncLogModelFactoryMock;
        private EventSyncer _eventSyncer;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EventSyncer>>();
            _syncLogRepositoryMock = new Mock<ISyncLogRepository>();
            _octopusRepositoryMock = new Mock<IOctopusRepository>();
            _releaseRepositoryMock = new Mock<IGenericRepository<ReleaseModel>>();
            _deploymentRepositoryMock = new Mock<IGenericRepository<DeploymentModel>>();
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
    }
}