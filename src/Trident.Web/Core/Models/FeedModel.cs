using System.ComponentModel.DataAnnotations.Schema;

namespace Trident.Web.Core.Models
{
    [Table("Feed")]
    public class FeedModel : BaseModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string RegExMatchingItems { get; set; }
    }
}