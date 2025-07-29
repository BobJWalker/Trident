using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/instances/{instanceId}/spaces/{spaceId}/projects/{projectId}/releases/{releaseId}/deployments")]
    public class DeploymentController(IGenericRepository<DeploymentModel> repository) : ControllerBase
    {        
        [HttpGet]        
        public Task<PagedViewModel<DeploymentModel>> GetAll(int releaseId, int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Start", bool isAsc = true)
        {
            return repository.GetAllByParentIdAsync(currentPage, rowsPerPage, sortColumn, isAsc, "releaseId", releaseId);
        }
        
        [HttpGet]
        [Route("{id}")]
        public Task<DeploymentModel> GetById(int id)
        {
            return repository.GetByIdAsync(id);
        }
        
        [HttpPost]        
        public Task<DeploymentModel> Insert(int releaseId, DeploymentModel model)
        {
            model.ReleaseId = releaseId;

            return repository.InsertAsync(model);
        }
        
        [HttpPut]
        [Route("{id}")]
        public Task<DeploymentModel> Update(int releaseId, int id, DeploymentModel model)
        {
            model.ReleaseId = releaseId;
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