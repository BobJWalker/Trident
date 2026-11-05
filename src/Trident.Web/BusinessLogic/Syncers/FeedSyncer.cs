using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Trident.Web.DataAccess;
using Trident.Web.HostedServices;

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
        ILogger<SyncJobHostedService> logger)
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
            }
        }
    }
}
