using Trident.Web.Core.Models;

namespace Trident.Web.Core.ViewModels
{
    public class HomeViewModel
    {
        public PagedViewModel<ArticleModel> ArticleList { get; set; }
        public bool ShowFeedList { get; set; }
    }
}
