using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Test.Fakes;

namespace Trident.Web.Test.BusinessLogic.Factories
{
    [TestFixture]
    public class FeedListFactoryTests
    {
        private FakeTridentConfiguration _tridentConfiguration;
        private FeedListFactory _feedListFactory;

        [SetUp]
        public void SetUp()
        {
            _tridentConfiguration = new FakeTridentConfiguration();

            _feedListFactory = new FeedListFactory(_tridentConfiguration);
        }

        [Test]
        public void GetFeedList_ShouldReturn_TwoItemsFromSampleFile()
        {
            // Arrange


            // Act
            var result = _feedListFactory.GetFeedList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));            
        }
    }
}
