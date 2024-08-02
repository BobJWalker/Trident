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

        [Test]
        public void ConvertFromOctopusToEnvironmentModel_ShouldReturnEnvironmentModelWithCorrectProperties()
        {
            // Arrange
            var octopusModel = new NameOnlyOctopusModel
            {
                Id = "Environments-1",
                Name = "Test Environment"
            };
            var spaceId = 2;

            // Act
            var result = _converter.ConvertFromOctopusToEnvironmentModel(octopusModel, spaceId);

            // Assert
            Assert.AreEqual(octopusModel.Name, result.Name);
            Assert.AreEqual(octopusModel.Id, result.OctopusId);
            Assert.AreEqual(spaceId, result.SpaceId);
        }

        [Test]
        public void ConvertFromOctopusToTenantModel_ShouldReturnCorrectTenantModel()
        {
            // Arrange
            var tenantOctopusModel = new NameOnlyOctopusModel
            {
                Id = "Tenants-1",
                Name = "Test Tenant"
            };
            var spaceId = 2;

            // Act
            var result = _converter.ConvertFromOctopusToTenantModel(tenantOctopusModel, spaceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(tenantOctopusModel.Name, result.Name);
            Assert.AreEqual(tenantOctopusModel.Id, result.OctopusId);
            Assert.AreEqual(spaceId, result.SpaceId);
        }

        [Test]
        public void ConvertFromOctopusToReleaseModel_ShouldReturnCorrectReleaseModel()
        {
            // Arrange
            var releaseOctopusModel = new ReleaseOctopusModel
            {
                Version = "1.0.0",
                Assembled = new DateTime(2022, 1, 1),
                Id = "Releases-1"
            };
            var projectId = 123;

            // Act
            var result = _converter.ConvertFromOctopusToReleaseModel(releaseOctopusModel, projectId);

            // Assert
            Assert.That(result.Version, Is.EqualTo("1.0.0"));
            Assert.That(result.Created, Is.EqualTo(new DateTime(2022, 1, 1)));
            Assert.That(result.OctopusId, Is.EqualTo("Releases-1"));
            Assert.That(result.ProjectId, Is.EqualTo(123));
        }
    }
}