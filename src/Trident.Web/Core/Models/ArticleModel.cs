using System.ComponentModel.DataAnnotations.Schema;

namespace Trident.Web.Core.Models
{
    [Table("Article")]
    public class ArticleModel : BaseModel
    {        
        public string Title { get; set; }
        public string Url { get; set; }
        public string FeedName { get; set; }
        public string ImageUrl { get; set; }
    }
}