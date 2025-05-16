using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class ReportController(IInstanceRepository instanceRepository, ISpaceRepository spaceRepository)
        : Controller
    {        
        public async Task<IActionResult> Index(int instanceId)
        {
            var instanceModel = await instanceRepository.GetByIdAsync(instanceId);
            var spaceList = await spaceRepository.GetAllAsync(currentPageNumber: 1, rowsPerPage: int.MaxValue, "Name", true, instanceId);

            var viewModel = new ReportingViewModel
            {
                Instance = instanceModel,
                SpaceList = spaceList.Items
            };

            return View(viewModel);
        }
    }
}