using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class SyncController(
        ITridentDataAdapter tridentDataAdapter,
        ISyncModelFactory syncModelFactory) : Controller
    {
        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "ID", bool isAsc = false)
        {
            var pagedSyncView = await tridentDataAdapter.GetAllAsync<SyncModel>(currentPage, rowsPerPage, sortColumn, isAsc);

            return View(pagedSyncView);
        }

        public async Task<IActionResult> StartSync()
        {
            var newSync = syncModelFactory.CreateModel(string.Empty);

            await tridentDataAdapter.InsertAsync(newSync);

            return RedirectToAction("Index", "Sync");
        }
    }
}
