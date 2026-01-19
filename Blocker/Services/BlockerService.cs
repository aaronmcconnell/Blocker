
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Blocker.Settings;

namespace Blocker.Services;

public class BlockerService : IHostedService
{
    private readonly BlockerServiceSettings _settings;
    private readonly IHostsFileService _hostsFileService;
    private readonly ICacheFlushService _cacheFlushService;
    private readonly ILogger<BlockerService> _logger;
    private readonly IStatusCheckService _statusCheckService;

    public BlockerService(IOptions<BlockerServiceSettings> settings, IHostsFileService hostsFileService, ICacheFlushService cacheFlushService,
        ILogger<BlockerService> logger, IStatusCheckService statusCheckService)
    {
        _logger = logger;
        _settings = settings.Value;
        _hostsFileService = hostsFileService;
        _cacheFlushService = cacheFlushService;
        _statusCheckService = statusCheckService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting {nameof(BlockerService)}...");

        foreach (var uriToBlock in _settings.UrisToBlock)
        {
            _logger.LogInformation("Ensuring {uri} status is correct", uriToBlock.UriToBlock);
            _statusCheckService.EnsureCorrectStatus(uriToBlock);    
        }

        _logger.LogInformation($"Started {nameof(BlockerService)}.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Stopping {nameof(BlockerService)}...");

        foreach (var uriToBlock in _settings.UrisToBlock)
        {
            if (_hostsFileService.IsRevoked(uriToBlock.UriToBlock))
            {
                _hostsFileService.Grant(uriToBlock.UriToBlock);
                _cacheFlushService.Flush();
            }            
        }

        _logger.LogInformation($"Stopped {nameof(BlockerService)}.");
        return Task.CompletedTask;
    }
}