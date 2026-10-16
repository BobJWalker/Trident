using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trident.Web.Core.Models
{    
    public class FeedModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> ArticleBaseUrls { get; set; }
    }
}