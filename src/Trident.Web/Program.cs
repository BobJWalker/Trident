using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Web;
using Octopus.OpenFeature.Provider;
using OpenFeature;
using OpenFeature.Model;
using OpenFeature.Contrib.Providers.EnvVar;
using SumoLogic.Logging.NLog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Trident.Web.HostedServices;
using Trident.Web.Core.Configuration;
using Trident.Web.DataAccess;

namespace Trident.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureOpenFeature(builder).GetAwaiter().GetResult();
            ConfigureConfigurationFiles(builder);
            ConfigureDependencyInjection(builder);
            ConfigureLogging(builder);
            ConfigureServices(builder);

            builder.Host.UseNLog();

            var app = builder.Build();
            app.UseDeveloperExceptionPage();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddWebOptimizer(minifyJavaScript: false, minifyCss: false);
            }
            else
            {
                builder.Services.AddWebOptimizer();
            }

            builder.Services.AddHostedService<SyncJobHostedService>();
        }

        private static void ConfigureConfigurationFiles(WebApplicationBuilder builder)
        {
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
        }

        private static void ConfigureDependencyInjection(WebApplicationBuilder builder)
        {
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(autofacBuilder =>
                {
                    autofacBuilder.RegisterInstance(GetFeatureClient())
                        .As<IFeatureClient>()
                        .SingleInstance();

                    autofacBuilder.RegisterInstance(GetMetricConfiguration())
                        .As<IMetricConfiguration>()
                        .SingleInstance();

                    autofacBuilder.RegisterInstance(GetDataAdapter())
                        .As<ITridentDataAdapter>()
                        .SingleInstance();

                    var assembly = Assembly.GetExecutingAssembly();

                    autofacBuilder.RegisterAssemblyTypes(assembly)
                        .Where(type => !type.IsAssignableTo<IHostedService>())
                        .AsImplementedInterfaces();

                    autofacBuilder.RegisterType<LoggerFactory>()
                        .As<ILoggerFactory>()
                        .InstancePerLifetimeScope();

                    autofacBuilder.RegisterGeneric(typeof(Logger<>))
                        .As(typeof(ILogger<>))
                        .InstancePerLifetimeScope();                    
                });
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            var sumoUrl = Environment.GetEnvironmentVariable("TRIDENT_SUMOLOGIC_URL");

            if (string.IsNullOrWhiteSpace(sumoUrl))
            {
                return;
            }

            if (sumoUrl == "blah")
            {
                return;
            }

            var logConfig = new LoggingConfiguration();
            var sumoTarget = new SumoLogicTarget();

            sumoTarget.Name = "SumoLogic";
            sumoTarget.Url = Environment.GetEnvironmentVariable("TRIDENT_SUMOLOGIC_URL");
            sumoTarget.SourceName = $"Trident.{Environment.GetEnvironmentVariable("TRIDENT_ENVIRONMENT")}";
            sumoTarget.SourceCategory = "trident";
            sumoTarget.ConnectionTimeout = 30000;
            sumoTarget.UseConsoleLog = true;
            sumoTarget.Layout = "${LEVEL}, ${message}${exception:format=tostring}${newline}";

            logConfig.AddTarget(sumoTarget);
            logConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, sumoTarget));

            builder.Logging.AddNLogWeb(logConfig);
        }

        private static async Task ConfigureOpenFeature(WebApplicationBuilder builder)
        {
            var clientIdentifier = Environment.GetEnvironmentVariable("TRIDENT_OPEN_FEATURE_CLIENT_ID");

            if (builder.Environment.IsDevelopment())
            {
                await OpenFeature.Api.Instance.SetProviderAsync(new EnvVarProvider("FeatureToggle_"));
            }
            else
            {
                await OpenFeature.Api.Instance.SetProviderAsync(new OctopusFeatureProvider(new OctopusFeatureConfiguration(clientIdentifier)));
            }
        }

        private static IFeatureClient GetFeatureClient()
        {
            var client = OpenFeature.Api.Instance.GetClient();

            client.SetContext(EvaluationContext.Builder().Build());

            return client;
        }

        private static IMetricConfiguration GetMetricConfiguration()
        {
            return new MetricConfiguration();
        }

        public static ITridentDataAdapter GetDataAdapter()
        {
            return new TridentDataAdapter(GetMetricConfiguration());
        }
    }
}
