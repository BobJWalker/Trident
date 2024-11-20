using Autofac;
using Autofac.Extensions.DependencyInjection;
using NLog.Config;
using NLog.Web;
using Octopus.OpenFeature.Provider;
using OpenFeature;
using SumoLogic.Logging.NLog;
using System.Reflection;
using Trident.Web.Core.FeatureToggles;
using Trident.Web.HostedServices;

var builder = WebApplication.CreateBuilder(args);

ConfigureConfigurationFiles(builder);
ConfigureDependencyInjection(builder);
ConfigureLogging(builder);
ConfigureServices(builder);

builder.Host.UseNLog();

await InitializeFeatureToggles();           

var app = builder.Build();
app.UseDeveloperExceptionPage();            
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
return;

static void ConfigureServices(WebApplicationBuilder builder)
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

static void ConfigureConfigurationFiles(WebApplicationBuilder builder)
{
    builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    builder.Configuration.AddEnvironmentVariables();
}

static void ConfigureDependencyInjection(WebApplicationBuilder builder)
{
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(autofacBuilder =>
        {
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

            autofacBuilder.Register(_ => OpenFeature.Api.Instance.GetClient())
                .As<IFeatureClient>()
                .InstancePerLifetimeScope();
        });
}

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

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

    builder.Logging.AddNLog(logConfig);
}

static async Task InitializeFeatureToggles()
{
    var octopusFeatureTogglesClientId = Environment.GetEnvironmentVariable("TRIDENT_FEATURE_TOGGLES_CLIENT_ID");

    if (string.IsNullOrEmpty(octopusFeatureTogglesClientId))
    {
        await OpenFeature.Api.Instance.SetProviderAsync(new EnvironmentFeatureToggleFeatureProvider());
    }
    else
    {
        await OpenFeature.Api.Instance.SetProviderAsync(new OctopusFeatureProvider(new OctopusFeatureConfiguration(octopusFeatureTogglesClientId)));
    }
}   