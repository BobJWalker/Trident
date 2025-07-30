using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trident.Web.BusinessLogic.Factories;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers
{
    public class HomeController (ILogger<HomeController> logger,
            ITridentDataAdapter tridentDataAdapter,
            ISyncModelFactory syncModelFactory,            
            IMetricConfiguration metricConfiguration)
        : Controller
    {        
        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Name", bool isAsc = true)
        {            
            var allInstances = await tridentDataAdapter.GetAllAsync<InstanceModel>(currentPage, rowsPerPage, sortColumn, isAsc);      

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

                await tridentDataAdapter.InsertAsync(defaultInstance);

                allInstances = await tridentDataAdapter.GetAllAsync<InstanceModel>(currentPage, rowsPerPage, sortColumn, isAsc);
            }
            else
            {
                logger.LogDebug("Instances found");
            }

            return View(allInstances);
        }

        public async Task<IActionResult> StartSync(int id)
        {
            var instance = await tridentDataAdapter.GetByIdAsync<InstanceModel>(id);

            var syncWhereClause = $"Where InstanceId = {id} and state = '{SyncState.Completed}'";
            var previousSync = await tridentDataAdapter.GetFirstRecordAsync<SyncModel>(syncWhereClause, sortColumn: "Completed", isAsc: false);

            var newSync = syncModelFactory.CreateModel(id, instance.Name, previousSync);

            await tridentDataAdapter.InsertAsync(newSync);

            return RedirectToAction("Index", "Sync");
        }

        public IActionResult AddInstance()
        {
            var instance = new InstanceModel();

            return View("InstanceMaintenance", instance);
        }

        public async Task<IActionResult> EditInstance(int id)
        {
            var instance = await tridentDataAdapter.GetByIdAsync<InstanceModel>(id);

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
                await tridentDataAdapter.UpdateAsync(model);
            }
            else
            {
                await tridentDataAdapter.InsertAsync(model);
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
