using Trident.Web.BusinessLogic.Converters;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.OctopusServerModels;

namespace Trident.Web.BusinessLogic.Tests.Converters
{
    [TestFixture]
    public class OctopusModelToInsightModelConverterTests
    {
        private IOctopusModelToInsightModelConverter _converter;

        [SetUp]
        public void Setup()
        {
            _converter = new OctopusModelToInsightModelConverter();
        }

        [Test]
        public void ConvertFromOctopusToDeploymentModel_ShouldReturnValidDeploymentModel()
        {
            // Arrange
            var deploymentOctopusModel = new DeploymentOctopusModel
            {
                Id = "Deployments-1",
                Name = "Deployment 1",
                EnvironmentId = "Environment1",
                TenantId = "Tenant1"
            };

            var deploymentOctopusTaskModel = new DeploymentOctopusTaskModel
            {
                QueueTime = new DateTime(2022, 1, 1),
                StartTime = new DateTime(2022, 1, 2),
                CompletedTime = new DateTime(2022, 1, 3),
                State = "Success"
            };

            var releaseId = 1;

            var environmentDictionary = new Dictionary<string, EnvironmentModel>
            {
                { "Environment1", new EnvironmentModel { Id = 1 } }
            };

            var tenantDictionary = new Dictionary<string, TenantModel>
            {
                { "Tenant1", new TenantModel { Id = 1 } }
            };

            // Act
            var result = _converter.ConvertFromOctopusToDeploymentModel(deploymentOctopusModel, deploymentOctopusTaskModel, releaseId, environmentDictionary, tenantDictionary);

            // Assert
            Assert.AreEqual(deploymentOctopusModel.Id, result.OctopusId);
            Assert.AreEqual(releaseId, result.ReleaseId);
            Assert.AreEqual(deploymentOctopusModel.Name, result.Name);
            Assert.AreEqual(environmentDictionary[deploymentOctopusModel.EnvironmentId].Id, result.EnvironmentId);
            Assert.AreEqual(tenantDictionary[deploymentOctopusModel.TenantId].Id, result.TenantId);
            Assert.AreEqual(deploymentOctopusTaskModel.QueueTime, result.QueueTime);
            Assert.AreEqual(deploymentOctopusTaskModel.StartTime, result.StartTime);
            Assert.AreEqual(deploymentOctopusTaskModel.CompletedTime, result.CompletedTime);
            Assert.AreEqual(deploymentOctopusTaskModel.State, result.DeploymentState);
        }

        [Test]
        public void ConvertFromOctopusToSpaceModel_ShouldReturnSpaceModelWithCorrectProperties()
        {
            // Arrange
            var nameOnlyOctopusModel = new NameOnlyOctopusModel
            {
                Id = "spaces-1",
                Name = "Test Space"
            };

            // Act
            var spaceModel = _converter.ConvertFromOctopusToSpaceModel(nameOnlyOctopusModel);

            // Assert
            Assert.AreEqual(nameOnlyOctopusModel.Id, spaceModel.OctopusId);
            Assert.AreEqual(nameOnlyOctopusModel.Name, spaceModel.Name);
        }

        [Test]
        public void ConvertFromOctopusToProjectModel_ShouldReturnCorrectProjectModel()
        {
            // Arrange
            var projectOctopusModel = new NameOnlyOctopusModel
            {
                Id = "Projects-1",
                Name = "Test Project"
            };
            var spaceId = 10;

            // Act
            var result = _converter.ConvertFromOctopusToProjectModel(projectOctopusModel, spaceId);

            // Assert
            Assert.AreEqual(projectOctopusModel.Name, result.Name);
            Assert.AreEqual(projectOctopusModel.Id, result.OctopusId);
            Assert.AreEqual(spaceId, result.SpaceId);
        }
    }
}