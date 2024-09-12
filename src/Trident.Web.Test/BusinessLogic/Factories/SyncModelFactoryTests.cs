using NUnit.Framework;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;

namespace Trident.Web.BusinessLogic.Tests.Factories
{
    [TestFixture]
    public class SyncModelFactoryTests
    {
        [Test]
        public void CreateModel_ReturnsSyncModelWithCorrectProperties()
        {
            // Arrange
            int instanceId = 1;
            string instanceName = "TestInstance";
            SyncModel previousSync = new SyncModel { Started = new DateTime(2022, 1, 1) };
            SyncModelFactory factory = new SyncModelFactory();

            // Act
            SyncModel result = factory.CreateModel(instanceId, instanceName, previousSync);

            // Assert
            Assert.That(result.InstanceId, Is.EqualTo(instanceId));
            Assert.That(result.Name, Is.EqualTo($"Sync for {instanceName}"));
            Assert.That(result.State, Is.EqualTo(SyncState.Queued));
            Assert.That(result.SearchStartDate, Is.EqualTo(previousSync.Started));
        }
    }
}