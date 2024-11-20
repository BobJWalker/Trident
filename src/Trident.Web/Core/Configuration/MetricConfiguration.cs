namespace Trident.Web.Core.Configuration
{
    public interface IMetricConfiguration
    {
        string ConnectionString { get; }
        string DefaultInstanceUrl { get; }
        string DefaultInstanceApiKey { get; }
        string DefaultInstanceId { get; }
    }

    public class MetricConfiguration : IMetricConfiguration
    {
        public string ConnectionString { get; }
        public string DefaultInstanceUrl {get; }
        public string DefaultInstanceApiKey {get; }
        public string DefaultInstanceId {get; }

        public MetricConfiguration()
        {
            ConnectionString = Environment.GetEnvironmentVariable("TRIDENT_CONNECTION_STRING") ?? "";  
            DefaultInstanceUrl = Environment.GetEnvironmentVariable("TRIDENT_INSTANCE_URL") ?? "";  
            DefaultInstanceApiKey = Environment.GetEnvironmentVariable("TRIDENT_INSTANCE_API_KEY") ?? "";   
            DefaultInstanceId = Environment.GetEnvironmentVariable("TRIDENT_INSTANCE_ID") ?? "";                 
        }
    }
}
