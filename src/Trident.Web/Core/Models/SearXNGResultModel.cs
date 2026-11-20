using System.Collections.Generic;

namespace Trident.Web.Core.Models
{
    public class SearXNGResultModel
    {
        public string Query { get; set; }
        public List<ArticleModel> results { get; set; }     
    }
}
