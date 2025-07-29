using Microsoft.Extensions.Logging;
using Moq;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.DataAccess;
using Trident.Web.BusinessLogic.Syncers;

namespace Trident.Web.Tests.BusinessLogic.Syncers
{
    [TestFixture]
    public class EventSyncerTest
    {
        private Mock<ILogger<EventSyncer>> _loggerMock;        
        private Mock<IOctopusRepository> _octopusRepositoryMock;
        private Mock<IGenericRepository> _genericRepoMock;        
        private Mock<ISyncLogModelFactory> _syncLogModelFactoryMock;
        private EventSyncer _eventSyncer;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EventSyncer>>();            
            _octopusRepositoryMock = new Mock<IOctopusRepository>();
            _genericRepoMock = new Mock<IGenericRepository>();            
            _syncLogModelFactoryMock = new Mock<ISyncLogModelFactory>();

            _eventSyncer = new EventSyncer(
                _loggerMock.Object,                
                _octopusRepositoryMock.Object,
                _genericRepoMock.Object,                
                _syncLogModelFactoryMock.Object
            );
        }
    }
}