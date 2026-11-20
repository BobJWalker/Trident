using System;
using System.Collections.Generic;
using System.Text;
using Trident.Web.Core.Configuration;

namespace Trident.Web.Test.Fakes
{
    internal class FakeTridentConfiguration : ITridentConfiguration
    {
        public string ConnectionString { get; set; }
        public string EnvironmentName { get; set; }
        public string ConfigurationFileName { get; set; }
        public string ConfigurationFilePath { get; set; }

        public FakeTridentConfiguration() 
        {
            ConnectionString = "blah";
            EnvironmentName = "blah";
            ConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory());
            ConfigurationFileName = "utah";
        }

        public string GetConfigurationFile()
        {
            return Path.Combine(ConfigurationFilePath, $"{ConfigurationFileName}.json");
        }
    }
}
