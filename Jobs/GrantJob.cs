using Microsoft.Extensions.Logging;
using Quartz;
using Blocker.Services;

namespace Blocker.Jobs;

public class GrantJob : IJob
{
    private readonly IHostsFileService _hostsFileService;
    private readonly ICacheFlushService _cacheFlushService;
    private readonly ILogger<GrantJob> _logger;

    public GrantJob(IHostsFileService hostsFileService, ICacheFlushService cacheFlushService, ILogger<GrantJob> logger)
    {
        _hostsFileService = hostsFileService;
        _cacheFlushService = cacheFlushService;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Running {jobName}...", nameof(GrantJob));

        var uri = context.MergedJobDataMap["uri"].ToString()
            ?? throw new ArgumentNullException("uri", "Job data does not contain a uri entry");

        if (_hostsFileService.IsRevoked(uri))
        {
            _hostsFileService.Grant(uri);
            _cacheFlushService.Flush();
            _logger.LogInformation("Grant applied to {uri}", uri);
        }

        _logger.LogDebug("{jobName} completed", nameof(GrantJob));

        return Task.CompletedTask;
    }
}