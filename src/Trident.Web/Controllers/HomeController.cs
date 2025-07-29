using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;
using Trident.Web.Core.Configuration;

namespace Trident.Web.Controllers
{
    public class HomeController (ILogger<HomeController> logger,
            IGenericRepository instanceRepository,
            ISyncModelFactory syncModelFactory,
            ISyncRepository syncRepository,
            IMetricConfiguration metricConfiguration)
        : Controller
    {        
        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Name", bool isAsc = true)
        {            
            var allInstances = await instanceRepository.GetAllAsync<InstanceModel>(currentPage, rowsPerPage, sortColumn, isAsc);      

            if (allInstances.Items.Count == 0 && string.IsNullOrWhiteSpace(metricConfiguration.DefaultInstanceUrl) == false && metricConfiguration.DefaultInstanceUrl != "blah")  
            {
                logger.LogDebug("No instances found, creating default instance");
                var defaultInstance = new InstanceModel
                {
                    Name = "Default Instance",
                    OctopusId = metricConfiguration.DefaultInstanceId,
                    Url = metricConfiguration.DefaultInstanceUrl,
                    ApiKey = metricConfiguration.DefaultInstanceApiKey
                };

                await instanceRepository.InsertAsync(defaultInstance);

                allInstances = await instanceRepository.GetAllAsync<InstanceModel>(currentPage, rowsPerPage, sortColumn, isAsc);
            }
            else
            {
                logger.LogDebug("Instances found");
            }

            return View(allInstances);
        }

        public async Task<IActionResult> StartSync(int id)
        {
            var instance = await instanceRepository.GetByIdAsync<InstanceModel>(id);
            var previousSync = await syncRepository.GetLastSuccessfulSync(id);

            var newSync = syncModelFactory.CreateModel(id, instance.Name, previousSync);

            await syncRepository.InsertAsync(newSync);

            return RedirectToAction("Index", "Sync");
        }

        public IActionResult AddInstance()
        {
            var instance = new InstanceModel();

            return View("InstanceMaintenance", instance);
        }

        public async Task<IActionResult> EditInstance(int id)
        {
            var instance = await instanceRepository.GetByIdAsync<InstanceModel>(id);

            return View("InstanceMaintenance", instance);
        }

        [HttpPost]
        public async Task<IActionResult> Save(InstanceModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View("InstanceMaintenance", model);
            }

            if (model.Id > 0)
            {
                await instanceRepository.UpdateAsync(model);
            }
            else
            {
                await instanceRepository.InsertAsync(model);
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
