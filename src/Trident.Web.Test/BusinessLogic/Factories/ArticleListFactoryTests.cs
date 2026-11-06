using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Trident.Web.Test.Fakes;

namespace Trident.Web.Test.BusinessLogic.Factories
{
    [TestFixture]
    public class ArticleListFactoryTests
    {
        private ILogger<ArticleListFactory> _logger;
        private ArticleListFactory _articleListFactory;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<ArticleListFactory>>().Object;

            _articleListFactory = new ArticleListFactory(_logger);
        }

        [Test]
        public void MakeArticleModelListFromWebContent_ForKSL_ReturnsAnArticleList()
        {
            // Arrange
            var feedModel = new FeedModel
            {
                ArticleBaseUrls = new List<string>
                {
                    "https://sports.ksl.com/nhl/utah-mammoth/"
                }
            };

            var content = FakeArticleContent.GetKSLArticleContent();

            // Act
            var result = _articleListFactory.MakeArticleModelListFromWebContent(feedModel, content);

            // Assert
            Assert.That(result.Count, Is.GreaterThan(1));
            Assert.That(result[0].Url.ToLowerInvariant(), Is.EqualTo("https://sports.ksl.com/nhl/utah-mammoth/utah-mammoth-sweep-toronto-maple-leafs/578717"));
            Assert.That(result[0].ImageUrl.ToLowerInvariant(), Is.EqualTo("https://img.ksl.com/slc/3207/320779/32077973.jpeg"));
            Assert.That(result[0].Title.ToLowerInvariant, Is.EqualTo("new lines spark offense as utah mammoth sweep toronto maple leafs to bounce back"));
        }
    }
}
