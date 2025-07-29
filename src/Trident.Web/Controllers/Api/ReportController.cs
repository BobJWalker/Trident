using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trident.Web.Core.Models.ViewModels;
using Trident.Web.DataAccess;

namespace Trident.Web.Controllers.Api
{
    [ApiController]
    [Route("api/instances/{instanceId}/reports")]
    public class ReportController(IGenericRepository genericRepository) : ControllerBase
    {
        [HttpPost]
        [Route("deploymentcounts")]
        public async Task<ReportResponseViewModel> GetDeploymentCounts(ReportRequestViewModel request)
        {
            return new ReportResponseViewModel
            {
                Data = await genericRepository.QueryAsync<ReportResponseDataViewModel>(
                        @"SELECT COUNT(*) AS [Count],
	                               CONVERT(VARCHAR, d.QueueTime, 1) Label
                            FROM dbo.Deployment d
	                            INNER JOIN dbo.Environment e
		                            ON d.EnvironmentId = e.Id
	                            INNER JOIN dbo.Release r
		                            ON d.ReleaseId = r.Id
	                            INNER JOIN dbo.Project p
		                            ON r.ProjectId = p.Id
	                            INNER JOIN dbo.Space s
		                            ON p.SpaceId = s.Id
	                            LEFT JOIN dbo.tenant t
		                            ON d.TenantId = t.Id
                            WHERE d.StartTime BETWEEN @StartDate AND @EndDate
                                and Isnull(@spaceId, s.id) = s.Id
                            GROUP BY CONVERT(VARCHAR, d.QueueTime, 1)
                            Order by CONVERT(VARCHAR, d.QueueTime, 1) asc",
                        new
                        {
                            StartDate = request.StartDate,
                            EndDate = request.EndDate.AddDays(1).AddSeconds(-1),
                            SpaceId = request.SpaceId <= 0 ? null : (int?)request.SpaceId
                        })
            };
        }

        [HttpPost]
        [Route("projectdeploycounts")]
        public async Task<ReportResponseViewModel> GetProjectDeploymentCounts(ReportRequestViewModel request)
        {
            return new ReportResponseViewModel
            {
                Data = await genericRepository.QueryAsync<ReportResponseDataViewModel>(
                        @"SELECT COUNT(*) AS [Count],
	                                p.Name Label
                            FROM dbo.Deployment d
	                            INNER JOIN dbo.Environment e
		                            ON d.EnvironmentId = e.Id
	                            INNER JOIN dbo.Release r
		                            ON d.ReleaseId = r.Id
	                            INNER JOIN dbo.Project p
		                            ON r.ProjectId = p.Id
	                            INNER JOIN dbo.Space s
		                            ON p.SpaceId = s.Id
	                            LEFT JOIN dbo.tenant t
		                            ON d.TenantId = t.Id
                            WHERE d.StartTime BETWEEN @StartDate AND @EndDate
                                and Isnull(@spaceId, s.id) = s.Id
                            GROUP BY p.Name
	                        ORDER BY COUNT(*) desc",
                        new
                        {
                            StartDate = request.StartDate,
                            EndDate = request.EndDate.AddDays(1).AddSeconds(-1),
                            SpaceId = request.SpaceId <= 0 ? null : (int?)request.SpaceId
                        })
            };
        }

        [HttpPost]
        [Route("environmentdeploycounts")]
        public async Task<ReportResponseViewModel> GetEnvironmentDeploymentCounts(ReportRequestViewModel request)
        {
            return new ReportResponseViewModel
            {
                Data = await genericRepository.QueryAsync<ReportResponseDataViewModel>(
                        @"SELECT COUNT(*) AS [Count],
	                                e.Name Label
                            FROM dbo.Deployment d
	                            INNER JOIN dbo.Environment e
		                            ON d.EnvironmentId = e.Id
	                            INNER JOIN dbo.Release r
		                            ON d.ReleaseId = r.Id
	                            INNER JOIN dbo.Project p
		                            ON r.ProjectId = p.Id
	                            INNER JOIN dbo.Space s
		                            ON p.SpaceId = s.Id
	                            LEFT JOIN dbo.tenant t
		                            ON d.TenantId = t.Id
                            WHERE d.StartTime BETWEEN @StartDate AND @EndDate
                                and Isnull(@spaceId, s.id) = s.Id
                            GROUP BY e.Name
	                        ORDER BY COUNT(*) desc",
                        new
                        {
                            StartDate = request.StartDate,
                            EndDate = request.EndDate.AddDays(1).AddSeconds(-1),
                            SpaceId = request.SpaceId <= 0 ? null : (int?)request.SpaceId
                        })
            };
        }
    }
}
