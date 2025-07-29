using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class ReportController(IGenericRepository genericRepository)
        : Controller
    {        
        public async Task<IActionResult> Index(int instanceId)
        {
            var instanceModel = await genericRepository.GetByIdAsync<InstanceModel>(instanceId);
            var spaceList = await genericRepository.GetAllByParentIdAsync<SpaceModel>(currentPageNumber: 1, rowsPerPage: int.MaxValue, "Name", true, "InstanceId", instanceId);

            var viewModel = new ReportingViewModel
            {
                Instance = instanceModel,
                SpaceList = spaceList.Items
            };

            return View(viewModel);
        }
    }
}