﻿using Microsoft.Extensions.Configuration;
using System;

namespace Octopus.Trident.Web.Core.Configuration
{
    public interface IMetricConfiguration
    {
        string ConnectionString { get; set; }
    }

    public class MetricConfiguration : IMetricConfiguration
    {
        public string ConnectionString { get; set; }

        public MetricConfiguration(IConfiguration configuration)
        {
            ConnectionString = Environment.GetEnvironmentVariable("TRIDENT_CONNECTION_STRING");
            
            if(string.IsNullOrWhiteSpace(ConnectionString))
            {
                ConnectionString = configuration["ConnectionStrings:Database"];
            }            
        }
    }
}
