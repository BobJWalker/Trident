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
            Assert.That(spaceModel.OctopusId, Is.EqualTo(nameOnlyOctopusModel.Id));
            Assert.That(spaceModel.Name, Is.EqualTo(nameOnlyOctopusModel.Name));
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
            Assert.That(result.Name, Is.EqualTo(projectOctopusModel.Name));
            Assert.That(result.OctopusId, Is.EqualTo(projectOctopusModel.Id));
            Assert.That(result.SpaceId, Is.EqualTo(spaceId));
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
            Assert.That(result.Name, Is.EqualTo(octopusModel.Name));
            Assert.That(result.OctopusId, Is.EqualTo(octopusModel.Id));
            Assert.That(result.SpaceId, Is.EqualTo(spaceId));
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
            Assert.That(result.Name, Is.EqualTo(tenantOctopusModel.Name));
            Assert.That(result.OctopusId, Is.EqualTo(tenantOctopusModel.Id));
            Assert.That(result.SpaceId, Is.EqualTo(spaceId));
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