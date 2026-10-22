namespace Trident.Web.Core.Models.ViewModels
{
    public class HomeViewModel
    {
        public PagedViewModel<ArticleModel> ArticleList { get; set; }
        public bool ShowFeedList { get; set; }
    }
}
