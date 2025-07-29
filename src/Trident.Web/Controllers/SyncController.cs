using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class SyncController(ISyncRepository syncRepository) : Controller
    {
        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Name", bool isAsc = true)
        {
            var pagedSyncView = await syncRepository.GetAllAsync(currentPage, rowsPerPage, sortColumn, isAsc);

            return View(pagedSyncView);
        }
    }
}
