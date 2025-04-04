﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using DbUp.Support;

namespace Trident.Database.DbUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args.FirstOrDefault(x => x.StartsWith("--ConnectionString", StringComparison.OrdinalIgnoreCase));
            connectionString = connectionString.Substring(connectionString.IndexOf("=") + 1).Replace(@"""", string.Empty);

            var executingPath = Assembly.GetExecutingAssembly().Location.Replace("Trident.Database.DbUp.dll", "").Replace("Trident.Database.DbUp.exe", "");
            Console.WriteLine($"The execution location is {executingPath}");

            var deploymentScriptPath = Path.Combine(executingPath, "DeploymentScripts");
            Console.WriteLine($"The deployment script path is located at {deploymentScriptPath}");

            var postDeploymentScriptsPath = Path.Combine(executingPath, "PostDeploymentScripts");
            Console.WriteLine($"The post deployment script path is located at {postDeploymentScriptsPath}");

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase(connectionString, null)                
                .WithScriptsFromFileSystem(deploymentScriptPath, new SqlScriptOptions { ScriptType = ScriptType.RunOnce, RunGroupOrder = 1 })
                .WithScriptsFromFileSystem(postDeploymentScriptsPath, new SqlScriptOptions { ScriptType = ScriptType.RunAlways, RunGroupOrder = 2 })
                .WithTransactionPerScript()
                .LogToConsole();

            var upgradeEngine = upgradeEngineBuilder.Build();
            Console.WriteLine("Is upgrade required: " + upgradeEngine.IsUpgradeRequired());
            
            if (args.Any(a => a.StartsWith("--PreviewReportPath", StringComparison.InvariantCultureIgnoreCase)))
            {
                GenerateReport(args, upgradeEngine);
            }
            else
            {
                var result = upgradeEngine.PerformUpgrade();

                // Display the result
                if (result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.WriteLine("Failed!");
                }
            }
        }

        private static void GenerateReport(string[] args, UpgradeEngine upgradeEngine)
        {
            Console.WriteLine("Generating a preview report for the upgrade");

            // Generate a preview file so Octopus Deploy can generate an artifact for approvals
            var report = args.FirstOrDefault(x => x.StartsWith("--PreviewReportPath", StringComparison.OrdinalIgnoreCase));
            report = report.Substring(report.IndexOf("=") + 1).Replace(@"""", string.Empty);

            if (Directory.Exists(report) == false)
            {
                Directory.CreateDirectory(report);
            }

            var fullReportPath = Path.Combine(report, "UpgradeReport.html");

            if (File.Exists(fullReportPath) == true)
            {
                File.Delete(fullReportPath);
            }

            Console.WriteLine($"Generating the report at {fullReportPath}");

            upgradeEngine.GenerateUpgradeHtmlReport(fullReportPath);
        }
    }
}