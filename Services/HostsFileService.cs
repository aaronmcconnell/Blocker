using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Blocker.Settings;

namespace Blocker.Services;

public class HostsFileService : IHostsFileService
{
    private readonly BlockerServiceSettings _settings;
    private readonly ILogger<HostsFileService> _logger;

    public HostsFileService(IOptions<BlockerServiceSettings> settings, ILogger<HostsFileService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public bool IsRevoked(string uri)
    {
        var lines = File.ReadAllLines(_settings.HostsFilePath);
        return lines.Contains(GetIPv4BlockEntry(uri)) || lines.Contains(GetIPv6BlockEntry(uri));
    }
    

    public void Grant(string uri)
    {
        if (!IsRevoked(uri)) return;
        var lines = File.ReadAllLines(_settings.HostsFilePath);
        var blockingLines = GetBlockEntries(uri);
        var updatedLines = lines.Except(blockingLines);

        try
        {
            File.WriteAllLines(_settings.HostsFilePath, updatedLines);
        }
        catch (UnauthorizedAccessException authEx)
        {
            _logger.LogError(authEx, "Blocker does not have write permissions on hosts file.");
        }
    }

    public void Revoke(string uri)
    {
        if (IsRevoked(uri)) return;
        var blockingLines = GetBlockEntries(uri);
        try
        {
            File.AppendAllLines(_settings.HostsFilePath, blockingLines);
        }
        catch (UnauthorizedAccessException authEx)
        {
            _logger.LogError(authEx, "Blocker does not have write permissions on hosts file.");
        }
    }

    private string GetIPv4BlockEntry(string uri)
    {
        return $"0.0.0.0 {uri}";
    }

    private string GetIPv6BlockEntry(string uri)
    {
        return $"::1 {uri}";
    }

    private string[] GetBlockEntries(string uri) {
        return [GetIPv4BlockEntry(uri), GetIPv6BlockEntry(uri)];
    }
}