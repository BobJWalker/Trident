using Microsoft.Extensions.Configuration;
using System;

namespace Trident.Web.Core.Configuration
{
    public interface IMetricConfiguration
    {
        string ConnectionString { get; set; }        
        string EnvironmentName { get; set; }
    }

    public class MetricConfiguration : IMetricConfiguration
    {
        public string ConnectionString { get; set; }        
        public string EnvironmentName { get; set; }

        public MetricConfiguration()
        {
            ConnectionString = Environment.GetEnvironmentVariable("TRIDENT_CONNECTION_STRING");                        
            EnvironmentName = Environment.GetEnvironmentVariable("TRIDENT_ENVIRONMENT");
        }
    }
}
