using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class ReportController(ITridentDataAdapter tridentDataAdapter)
        : Controller
    {        
        public async Task<IActionResult> Index(int instanceId)
        {
            var instanceModel = await tridentDataAdapter.GetByIdAsync<InstanceModel>(instanceId);
            var spaceList = await tridentDataAdapter.GetAllByParentIdAsync<SpaceModel>(currentPageNumber: 1, rowsPerPage: int.MaxValue, "Name", true, "InstanceId", instanceId);

            var viewModel = new ReportingViewModel
            {
                Instance = instanceModel,
                SpaceList = spaceList.Items
            };

            return View(viewModel);
        }
    }
}