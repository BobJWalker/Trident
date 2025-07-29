using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Trident.Web.Core.Configuration;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;
using Trident.Web.Core.Models.ViewModels;

namespace Trident.Web.DataAccess
{
    public interface ISyncRepository : IGenericRepository
    {        
        Task<PagedViewModel<SyncModel>> GetNextRecordsToProcessAsync();
        Task<SyncModel> GetLastSuccessfulSync(int instanceId);
    }

    public class SyncRepository(IMetricConfiguration metricConfiguration) : GenericRepository(metricConfiguration), ISyncRepository
    {
        public Task<PagedViewModel<SyncModel>> GetNextRecordsToProcessAsync()
        {
            const int rowsPerPage = 5;
            const int currentPageNumber = 1;
            var whereClause = $"Where State = '{SyncState.Queued}' and IsNull(RetryAttempts,0) < 5";
            return GetAllAsync<SyncModel>(currentPageNumber, rowsPerPage, "Created", isAsc: true, whereClause);
        }

        public async Task<SyncModel> GetLastSuccessfulSync(int instanceId)
        {
            using (var connection = new SqlConnection(metricConfiguration.ConnectionString))
            {
                return (await connection.GetListPagedAsync<SyncModel>(1, 1, $"Where InstanceId = {instanceId} and state = '{SyncState.Completed}'", "Completed Desc")).FirstOrDefault();
            }
        }
    }
}
