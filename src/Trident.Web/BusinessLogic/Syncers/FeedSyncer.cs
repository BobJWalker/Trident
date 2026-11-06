using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Trident.Web.DataAccess;

namespace Trident.Web.BusinessLogic.Syncers
{
    public interface IFeedSyncer
    {
        public Task SyncAllFeedsAsync(SyncModel syncJob);
    }

    public class FeedSyncer (
        ITridentWebAdapter tridentWebAdapter,
        ITridentDataAdapter tridentDataAdapter,
        IFeedListFactory feedListFactory,
        IArticleListFactory articleListFactory,
        ILogger<FeedSyncer> logger) : IFeedSyncer
    {        
        public async Task SyncAllFeedsAsync(SyncModel syncJob)
        {
            logger.LogInformation($"Beginning Syncjob {syncJob.Id} by getting list of feeds");

            var feedList = feedListFactory.GetFeedList();

            logger.LogInformation($"Found {feedList.Count} feeds to process");

            foreach (var feed in feedList)
            {
                logger.LogInformation($"Getting the contents of ${feed.Url}");
                var results = await tridentWebAdapter.GetWebResponseAsync(feed.Url);

                var articleList = articleListFactory.MakeArticleModelListFromWebContent(feed, results);

                logger.LogInformation($"Pulled ${articleList.Count} article(s) off the page");
                foreach (var article in articleList)
                {
                    var existingArticle = await GetExistingArticle(article);

                    if (existingArticle != null)
                    {
                        logger.LogInformation($"Article ${article.Title} already exists, skipping");
                        continue;
                    }

                    logger.LogInformation($"Adding article ${article.Title} to the database");
                    await tridentDataAdapter.InsertAsync(article);
                }
            }
        }

        private async Task<ArticleModel> GetExistingArticle(ArticleModel article)
        {
            var query = "Select * from Article where Url = @url";

            var existingArticle = await tridentDataAdapter.GetFirstRecordAsync<ArticleModel>(query, new { url = article.Url });

            if (existingArticle == null)
            {
                return null;
            }

            if (existingArticle.Id == 0)
            {
                return null;
            }

            return existingArticle;
        }
    }
}
