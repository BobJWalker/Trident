using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Models;

namespace Trident.Web.BusinessLogic.Factories
{
    public interface IFeedListFactory
    {
        List<FeedModel> GetFeedList();
    }

    public class FeedListFactory(ITridentConfiguration tridentConfiguration) : IFeedListFactory
    {        
        public List<FeedModel> GetFeedList()
        {
            var feedFile = tridentConfiguration.GetConfigurationFile();
            using (var fileStream = File.OpenRead(feedFile))
            {
                return JsonSerializer.Deserialize<List<FeedModel>>(fileStream);
            }            
        }
    }
}
