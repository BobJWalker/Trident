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
            Assert.That(result.Type, Is.EqualTo(LogType.Normal));
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.SyncId, Is.EqualTo(syncId));
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
            Assert.That(result.SyncId, Is.EqualTo(syncId));
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.Type, Is.EqualTo(LogType.Error));
        }

        [Test]
        public void MakeWarningLog_ShouldReturnWarningLog()
        {
            // Arrange
            var message = "Warning message";
            var syncId = 3;

            // Act
            var result = _syncLogModelFactory.MakeWarningLog(message, syncId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(syncId, result.SyncId);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(LogType.Warning, result.Type);
            Assert.True(result.Created <= DateTime.UtcNow);
        }
    }
}