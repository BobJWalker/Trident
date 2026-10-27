using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class HomeController (
        ITridentDataAdapter tridentDataAdapter, 
        ISyncModelFactory syncModelFactory,
        IFeedListFactory feedListFactory,
        ITridentFeatureFlag tridentFeatureFlag)
        : Controller
    {        
        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Id", bool isAsc = false)
        {

            var showFeedList = await tridentFeatureFlag.IsEnabledAsync(TridentFeatureFlagList.ViewFeedList);

            var pagedArticles = await tridentDataAdapter.GetAllAsync<ArticleModel>(currentPage, rowsPerPage, sortColumn, isAsc);

            var homeViewModel = new HomeViewModel
            {
                ShowFeedList = showFeedList,
                ArticleList = pagedArticles,
            };

            return View(homeViewModel);
        }

        public async Task<IActionResult> Feed()
        {
            var showFeedList = await tridentFeatureFlag.IsEnabledAsync(TridentFeatureFlagList.ViewFeedList);
            var allFeeds = showFeedList ? feedListFactory.GetFeedList() : new List<FeedModel>();

            return View(allFeeds);
        }

        public async Task<IActionResult> StartSync()
        {            
            var newSync = syncModelFactory.CreateModel(string.Empty);

            await tridentDataAdapter.InsertAsync(newSync);

            return RedirectToAction("Index", "Sync");
        }        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
