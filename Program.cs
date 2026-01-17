using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using Quartz;
using Blocker.DependencyInjection;
using Blocker.Extensions;
using Blocker.Services;
using Blocker.Settings;
using Serilog;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    ConsoleExtension.Hide();
}

string mutex_id = "Global\\BLOCKER";
using var mutex = new Mutex(false, mutex_id);

if (!mutex.WaitOne(0, false))
{
    Console.WriteLine("Blocker is already running");
    return;
}

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = new HostBuilder();

builder.ConfigureHostConfiguration((config) =>
{
    config.AddJsonFile("appsettings.json", false);
});

builder.ConfigureServices((services) =>
{
    services.AddLogging(lb =>
    {
        lb.AddSerilog();
    });

    services.AddOptions<BlockerServiceSettings>()
        .BindConfiguration(BlockerServiceSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddQuartz(q => q.AddJobs(services));

    services.AddSingleton<IHostsFileService, HostsFileService>();
    services.AddSingleton<ICacheFlushService, CacheFlushService>();
    services.AddSingleton<IStatusCheckService, StatusCheckService>();

    services.AddHostedService<BlockerService>(); 
});

var app = builder.Build();


var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
var schedule = await schedulerFactory.GetScheduler();

await schedule.Start();

await app.RunAsync();
