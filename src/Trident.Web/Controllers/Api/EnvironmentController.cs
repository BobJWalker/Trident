using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/instances/{instanceId}/spaces/{spaceId}/environments")]
    public class EnvironmentController(ITridentDataAdapter repository) : ControllerBase
    {
        [HttpGet]        
        public Task<PagedViewModel<EnvironmentModel>> GetAll(int spaceId, int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Start", bool isAsc = true)
        {
            return repository.GetAllByParentIdAsync<EnvironmentModel>(currentPage, rowsPerPage, sortColumn, isAsc, "SpaceId", spaceId);
        }

        [HttpGet]
        [Route("{id}")]
        public Task<EnvironmentModel> GetById(int id)
        {
            return repository.GetByIdAsync<EnvironmentModel>(id);
        }
        
        [HttpPost]        
        public Task<EnvironmentModel> Insert(int spaceId, EnvironmentModel model)
        {
            model.SpaceId = spaceId;

            return repository.InsertAsync<EnvironmentModel>(model);
        }
        
        [HttpPut]
        [Route("{id}")]
        public Task<EnvironmentModel> Update(int spaceId, int id, EnvironmentModel model)
        {
            model.SpaceId = spaceId;
            model.Id = id;
            return repository.UpdateAsync<EnvironmentModel>(model);
        }
        
        [HttpDelete]
        [Route("{id}")]
        public Task Delete(int id)
        {
            return repository.DeleteAsync<EnvironmentModel>(id);
        }
    }
}