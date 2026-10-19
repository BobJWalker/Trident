using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/articles")]
    public class ArticleController(ITridentDataAdapter repository) : ControllerBase
    {
        [HttpGet]        
        public Task<PagedViewModel<ArticleModel>> GetAll(int currentPage = 1, int rowsPerPage = 10, string sortColumn = "Id", bool isAsc = false)
        {
            return repository.GetAllAsync<ArticleModel>(currentPage, rowsPerPage, sortColumn, isAsc);
        }
        
        [HttpGet]
        [Route("{id}")]
        public Task<ArticleModel> GetById(int id)
        {
            return repository.GetByIdAsync<ArticleModel>(id);
        }       
        
        [HttpDelete]
        [Route("{id}")]
        public Task Delete(int id)
        {
            return repository.DeleteAsync<ArticleModel>(id);
        }
    }
}