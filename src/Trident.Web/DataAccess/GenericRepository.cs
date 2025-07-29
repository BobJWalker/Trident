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
    public interface IGenericRepository<T> where T : BaseOctopusModel
    {
        Task<PagedViewModel<T>> GetAllAsync(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc);
        Task<T> GetByIdAsync(int id);
        Task<T> GetByOctopusIdAsync(string octopusId);        
        Task<PagedViewModel<T>> GetAllByParentIdAsync(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc, string whereColumn, int parentId);
        Task<T> InsertAsync(T model);
        Task<T> UpdateAsync(T model);
        Task DeleteAsync(int id);
    }

    public class GenericRepository<T>(IMetricConfiguration metricConfiguration) : IGenericRepository<T> where T : BaseOctopusModel
    {
        public async Task<T> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                return await connection.GetAsync<T>(id);
            }
        }

        public async Task<PagedViewModel<T>> GetAllAsync(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc)
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

        public async Task<T> GetByOctopusIdAsync(string octopusId)
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var results = await connection.GetListAsync<T>(new { OctopusId = octopusId });

                return results.FirstOrDefault();
            }
        }

        public async Task<PagedViewModel<T>> GetAllByParentIdAsync(int currentPageNumber, int rowsPerPage, string sortColumn, bool isAsc, string whereColumn, int parentId)
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

        public async Task<T> InsertAsync(T model)
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                var id = await connection.InsertAsync(model);

                model.Id = id.GetValueOrDefault();

                return model;
            }
        }

        public async Task<T> UpdateAsync(T model)
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                await connection.UpdateAsync(model);

                return model;
            }
        }

        public async Task DeleteAsync(int id)
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