using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/instances/{instanceId}/spaces/{spaceId}/projects/{projectId}/releases")]
    public class ReleaseController(ITridentDataAdapter repository) : ControllerBase
    {        
        [HttpGet]
        public Task<PagedViewModel<ReleaseModel>> GetAll(int projectId, int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Start", bool isAsc = true)
        {
            return repository.GetAllByParentIdAsync<ReleaseModel>(currentPage, rowsPerPage, sortColumn, isAsc, "ProjectId", projectId);
        }

        [HttpGet]
        [Route("{id}")]
        public Task<ReleaseModel> GetById(int id)
        {
            return repository.GetByIdAsync<ReleaseModel>(id);
        }
        
        [HttpPost]        
        public Task<ReleaseModel> Insert(int projectId, ReleaseModel model)
        {
            model.ProjectId = projectId;

            return repository.InsertAsync(model);
        }
        
        [HttpPut]
        [Route("{id}")]
        public Task<ReleaseModel> Update(int projectId, int id, ReleaseModel model)
        {
            model.ProjectId = projectId;
            model.Id = id;
            return repository.UpdateAsync(model);
        }
        
        [HttpDelete]
        [Route("{id}")]
        public Task Delete(int projectId, int id)
        {
            return repository.DeleteAsync<ReleaseModel>(id);
        }
    }
}