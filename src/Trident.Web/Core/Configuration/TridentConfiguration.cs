using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Trident.Web.Core.Configuration
{
    public interface ITridentConfiguration
    {
        string ConnectionString { get; }        
        string EnvironmentName { get; }
        string ConfigurationFileName { get; }
        string ConfigurationFilePath { get; }

        string GetConfigurationFile();
    }

    public class TridentConfiguration : ITridentConfiguration
    {
        public string ConnectionString { get; set; }        
        public string EnvironmentName { get; set; }
        public string ConfigurationFileName { get; set; }
        public string ConfigurationFilePath { get; private set; }

        public TridentConfiguration()
        {
            ConnectionString = Environment.GetEnvironmentVariable("TRIDENT_CONNECTION_STRING");                        
            EnvironmentName = Environment.GetEnvironmentVariable("TRIDENT_ENVIRONMENT");
            ConfigurationFileName = Environment.GetEnvironmentVariable("TRIDENT_FILE_NAME");
            ConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
        }

        public string GetConfigurationFile()
        {
            return Path.Combine(ConfigurationFilePath, $"{ConfigurationFileName}.json");
        }
    }
}
