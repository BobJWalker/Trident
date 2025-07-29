using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;

namespace Trident.Web.DataAccess
{
    public interface IGenericRepository
    {
        Task<PagedViewModel<T>> GetAllAsync<T>(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc) where T : BaseModel;
        Task<T> GetByIdAsync<T>(int id) where T : BaseModel;
        Task<T> GetByOctopusIdAsync<T>(string octopusId) where T : BaseOctopusModel;
        Task<PagedViewModel<T>> GetAllByParentIdAsync<T>(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc, string whereColumn, int parentId) where T : BaseModel;
        Task<T> InsertAsync<T>(T model) where T : BaseModel;
        Task<T> UpdateAsync<T>(T model) where T : BaseModel;
        Task DeleteAsync<T>(int id) where T : BaseModel;
    }

    public class GenericRepository(IMetricConfiguration metricConfiguration) : IGenericRepository
    {
        public async Task<T> GetByIdAsync<T>(int id) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                return await connection.GetAsync<T>(id);
            }
        }

        public async Task<PagedViewModel<T>> GetAllAsync<T>(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var totalRecords = await connection.RecordCountAsync<T>(null);
                var results = await connection.GetListPagedAsync<T>(currentPageNumber, rowsPerPage, $"", $"{sortColumn} {(isAsc ? "asc" : "desc")}");

                return new PagedViewModel<T>
                {
                    Items = results.ToList(),
                    TotalPages = GetTotalPages(totalRecords, rowsPerPage),
                    TotalRecords = totalRecords,
                    CurrentPageNumber = currentPageNumber,
                    RowsPerPage = rowsPerPage
                };
            }
        }

        public async Task<PagedViewModel<T>> GetAllAsync<T>(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc, string whereClause) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var totalRecords = await connection.RecordCountAsync<T>(null);
                var results = await connection.GetListPagedAsync<T>(currentPageNumber, rowsPerPage, $"", $"{sortColumn} {(isAsc ? "asc" : "desc")}");

                return new PagedViewModel<T>
                {
                    Items = results.ToList(),
                    TotalPages = GetTotalPages(totalRecords, rowsPerPage),
                    TotalRecords = totalRecords,
                    CurrentPageNumber = currentPageNumber,
                    RowsPerPage = rowsPerPage
                };
            }
        }

        public async Task<T> GetByOctopusIdAsync<T>(string octopusId) where T : BaseOctopusModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var results = await connection.GetListAsync<T>(new { OctopusId = octopusId });

                return results.FirstOrDefault();
            }
        }

        public async Task<PagedViewModel<T>> GetAllByParentIdAsync<T>(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc, string whereColumn, int parentId) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var totalRecords = await connection.RecordCountAsync<T>(null);
                var results = await connection.GetListPagedAsync<T>(currentPageNumber, rowsPerPage, $"Where {whereColumn} = {parentId}", $"{sortColumn} {(isAsc ? "asc" : "desc")}");

                return new PagedViewModel<T>
                {
                    Items = results.ToList(),
                    TotalPages = GetTotalPages(totalRecords, rowsPerPage),
                    TotalRecords = totalRecords,
                    CurrentPageNumber = currentPageNumber,
                    RowsPerPage = rowsPerPage
                };
            }
        }

        public async Task<T> InsertAsync<T>(T model) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var id = await connection.InsertAsync(model);

                model.Id = id.GetValueOrDefault();

                return model;
            }
        }

        public async Task<T> UpdateAsync<T>(T model) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                await connection.UpdateAsync(model);

                return model;
            }
        }

        public async Task DeleteAsync<T>(int id) where T : BaseModel
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                await connection.DeleteAsync<T>(id);

                return;
            }
        }

        protected int GetTotalPages(int totalRecords, int rowsPerPage)
        {
            return Convert.ToInt32(Math.Ceiling((double)totalRecords) / rowsPerPage);
        }
    }
}