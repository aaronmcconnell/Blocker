using Microsoft.Extensions.Logging;
using Quartz;
using Blocker.Services;

namespace Blocker.Jobs;

public class RevokeJob : IJob
{
    private readonly IHostsFileService _hostsFileService;
    private readonly ICacheFlushService _cacheFlushService;
    private readonly ILogger<RevokeJob> _logger;

    public RevokeJob(IHostsFileService hostsFileService, ICacheFlushService cacheFlushService, ILogger<RevokeJob> logger)
    {
        _hostsFileService = hostsFileService;
        _cacheFlushService = cacheFlushService;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Running {jobName}...", nameof(RevokeJob));

        var uri = context.MergedJobDataMap["uri"].ToString()
            ?? throw new ArgumentNullException("uri", "Job data does not contain a uri entry");

        if (!_hostsFileService.IsRevoked(uri))
        {
            _hostsFileService.Revoke(uri);
            _cacheFlushService.Flush();
            _logger.LogInformation("Revoke applied to {uri}", uri);
        }

        _logger.LogDebug("{jobName} completed", nameof(RevokeJob));

        return Task.CompletedTask;
    }
}