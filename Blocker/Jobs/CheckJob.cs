using Microsoft.Extensions.Logging;
using Quartz;
using Blocker.Services;
using Blocker.Settings;
using Microsoft.Extensions.Options;

namespace Blocker.Jobs;

public class CheckJob : IJob
{
    private readonly ILogger<CheckJob> _logger;
    private readonly IStatusCheckService _statusCheckService;
    private readonly BlockerServiceSettings _settings;

    public CheckJob(ILogger<CheckJob> logger, IStatusCheckService statusCheckService, IOptions<BlockerServiceSettings> settings)
    {
        _logger = logger;
        _statusCheckService = statusCheckService;
        _settings = settings.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Running {jobName}...", nameof(CheckJob));

        var uri = context.MergedJobDataMap["uri"].ToString()
            ?? throw new ArgumentNullException("uri", "Job data does not contain a uri entry");

        var blockSetting = GetBlockSettingByUri(uri);

        await _statusCheckService.EnsureCorrectStatus(blockSetting);

        _logger.LogDebug("{jobName} completed", nameof(CheckJob));
    }

    private BlockUriSettings GetBlockSettingByUri(string uri)
    {
        return _settings.UrisToBlock.Single(u => u.UriToBlock == uri);
    }
}