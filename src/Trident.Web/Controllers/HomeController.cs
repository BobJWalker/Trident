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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInstanceRepository _instanceRepository;
        private readonly ISyncModelFactory _syncModelFactory;
        private readonly ISyncRepository _syncRepository;
        private readonly IMetricConfiguration _metricConfiguration;

        public HomeController(ILogger<HomeController> logger, 
            IInstanceRepository instanceRepository,
            ISyncModelFactory syncModelFactory,
            ISyncRepository syncRepository,
            IMetricConfiguration metricConfiguration)
        {
            _logger = logger;
            _instanceRepository = instanceRepository;
            _syncModelFactory = syncModelFactory;
            _syncRepository = syncRepository;
            _metricConfiguration = metricConfiguration;
        }

        public async Task<IActionResult> Index(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Name", bool isAsc = true)
        {            
            var allInstances = await _instanceRepository.GetAllAsync(currentPage, rowsPerPage, sortColumn, isAsc);      

            if (allInstances.Items.Count == 0 && string.IsNullOrWhiteSpace(_metricConfiguration.DefaultInstanceUrl) == false && _metricConfiguration.DefaultInstanceUrl != "blah")  
            {
                _logger.LogDebug("No instances found, creating default instance");
                var defaultInstance = new InstanceModel
                {
                    Name = "Default Instance",
                    OctopusId = _metricConfiguration.DefaultInstanceId,
                    Url = _metricConfiguration.DefaultInstanceUrl,
                    ApiKey = _metricConfiguration.DefaultInstanceApiKey
                };

                await _instanceRepository.InsertAsync(defaultInstance);

                allInstances = await _instanceRepository.GetAllAsync(currentPage, rowsPerPage, sortColumn, isAsc);
            }
            else
            {
                _logger.LogDebug("Instances found");
            }

            return View(allInstances);
        }

        public async Task<IActionResult> StartSync(int id)
        {
            var instance = await _instanceRepository.GetByIdAsync(id);
            var previousSync = await _syncRepository.GetLastSuccessfulSync(id);

            var newSync = _syncModelFactory.CreateModel(id, instance.Name, previousSync);

            await _syncRepository.InsertAsync(newSync);

            return RedirectToAction("Index", "Sync");
        }

        public IActionResult AddInstance()
        {
            var instance = new InstanceModel();

            return View("InstanceMaintenance", instance);
        }

        public async Task<IActionResult> EditInstance(int id)
        {
            var instance = await _instanceRepository.GetByIdAsync(id);

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
                await _instanceRepository.UpdateAsync(model);
            }
            else
            {
                await _instanceRepository.InsertAsync(model);
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
