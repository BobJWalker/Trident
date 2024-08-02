using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;
using Trident.Web.BusinessLogic.Factories;

namespace Trident.Web.BusinessLogic.Tests.Factories
{
    [TestFixture]
    public class SyncLogModelFactoryTests
    {
        private ISyncLogModelFactory _syncLogModelFactory;

        [SetUp]
        public void SetUp()
        {
            _syncLogModelFactory = new SyncLogModelFactory();
        }

        [Test]
        public void MakeInformationLog_ShouldReturnSyncLogModelWithInformationLogType()
        {
            // Arrange
            string message = "Information log message";
            int syncId = 1;

            // Act
            SyncLogModel result = _syncLogModelFactory.MakeInformationLog(message, syncId);

            // Assert
            Assert.AreEqual(LogType.Normal, result.Type);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(syncId, result.SyncId);
        }

        [Test]
        public void MakeErrorLog_ShouldReturnSyncLogModelWithErrorLogType()
        {
            // Arrange
            string message = "Error log message";
            int syncId = 1;

            // Act
            SyncLogModel result = _syncLogModelFactory.MakeErrorLog(message, syncId);

            // Assert
            Assert.AreEqual(syncId, result.SyncId);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(LogType.Error, result.Type);
        }
    }
}