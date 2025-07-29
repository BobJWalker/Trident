using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/instances/{instanceId}/spaces/{spaceId}/environments")]
    public class EnvironmentController(IGenericRepository<EnvironmentModel> repository) : ControllerBase
    {
        [HttpGet]        
        public Task<PagedViewModel<EnvironmentModel>> GetAll(int spaceId, int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Start", bool isAsc = true)
        {
            return repository.GetAllByParentIdAsync(currentPage, rowsPerPage, sortColumn, isAsc, "SpaceId", spaceId);
        }

        [HttpGet]
        [Route("{id}")]
        public Task<EnvironmentModel> GetById(int id)
        {
            return repository.GetByIdAsync(id);
        }
        
        [HttpPost]        
        public Task<EnvironmentModel> Insert(int spaceId, EnvironmentModel model)
        {
            model.SpaceId = spaceId;

            return repository.InsertAsync(model);
        }
        
        [HttpPut]
        [Route("{id}")]
        public Task<EnvironmentModel> Update(int spaceId, int id, EnvironmentModel model)
        {
            model.SpaceId = spaceId;
            model.Id = id;
            return repository.UpdateAsync(model);
        }
        
        [HttpDelete]
        [Route("{id}")]
        public Task Delete(int id)
        {
            return repository.DeleteAsync(id);
        }
    }
}