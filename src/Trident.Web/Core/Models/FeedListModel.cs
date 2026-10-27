using System.Collections.Generic;

namespace Trident.Web.Core.Models
{
    public class FeedListModel
    {
        public List<FeedModel> Feeds { get; set; } = new List<FeedModel>();
        public string ConfigurationName { get; set; }
        
    }
}
