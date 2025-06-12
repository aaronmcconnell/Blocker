using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Blocker.Settings;

namespace Blocker.Services;

public class StatusCheckService : IStatusCheckService
{
    private readonly IHostsFileService _hostsFileService;
    private readonly ICacheFlushService _cacheFlushService;
    private readonly BlockerServiceSettings _settings;
    private readonly ILogger<StatusCheckService> _logger;

    public StatusCheckService(IHostsFileService hostsFileService, ICacheFlushService cacheFlushService, IOptions<BlockerServiceSettings> settings,
        ILogger<StatusCheckService> logger)
    {
        _hostsFileService = hostsFileService;
        _cacheFlushService = cacheFlushService;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task EnsureCorrectStatus(BlockUriSettings blockSettings)
    {
        if (ShouldBeRevoked(blockSettings) && !_hostsFileService.IsRevoked(blockSettings.UriToBlock))
        {
            _logger.LogInformation("Misconfiguration detected on Startup: Should be revoked, but is granted.");
            _hostsFileService.Revoke(blockSettings.UriToBlock);
            _cacheFlushService.Flush();
            _logger.LogInformation("Revoke applied");
        }
        else if (!ShouldBeRevoked(blockSettings) && _hostsFileService.IsRevoked(blockSettings.UriToBlock))
        {
            _logger.LogInformation("Misconfiguration detected on Startup: Should be granted, but is revoked.");
            _hostsFileService.Grant(blockSettings.UriToBlock);
            _cacheFlushService.Flush();
            _logger.LogInformation("Grant applied");
        }
        return Task.CompletedTask;
    }

    private bool ShouldBeRevoked(BlockUriSettings blockSettings)
    {
        var ts = DateTimeOffset.Now;
        var activeDays = blockSettings.ActiveDays.Select(Enum.Parse<DayOfWeek>);

        if (!activeDays.Contains(ts.DayOfWeek))
            return false;

        var blockFrom = _settings.ReadTimeFromSetting(blockSettings.BlockFrom);
        var blockTo = _settings.ReadTimeFromSetting(blockSettings.BlockTo);
        var currentTime = new TimeOnly(ts.Hour, ts.Minute);

        return blockFrom < currentTime && blockTo > currentTime;
    }


}