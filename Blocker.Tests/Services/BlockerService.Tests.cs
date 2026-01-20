using AutoFixture;
using Blocker.Services;
using Blocker.Settings;
using Blocker.Tests.AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Blocker.Tests.Services;

public class BlockerServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IHostsFileService> _mockHostsFileService;
    private readonly Mock<ICacheFlushService> _mockCacheFlushService;
    private readonly Mock<ILogger<BlockerService>> _mockLogger;
    private readonly Mock<IStatusCheckService> _mockStatusCheckService;
    private readonly IOptions<BlockerServiceSettings> _mockSettings;

    public BlockerServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new BlockUriSettingsSpecimenBuilder());
        _mockCacheFlushService = new Mock<ICacheFlushService>();
        _mockHostsFileService = new Mock<IHostsFileService>();
        _mockLogger = new Mock<ILogger<BlockerService>>();
        _mockStatusCheckService = new Mock<IStatusCheckService>();
        _mockSettings = Options.Create(_fixture.Create<BlockerServiceSettings>());
    }

    [Fact]
    public async Task StartAsync_EnsuresCorrectStatusOnStartup()
    {
        var blockerService = new BlockerService(_mockSettings, _mockHostsFileService.Object, _mockCacheFlushService.Object,
            _mockLogger.Object, _mockStatusCheckService.Object);

        var cts = new CancellationTokenSource();

        await blockerService.StartAsync(cts.Token);

        var blockedUriCount = _mockSettings.Value.UrisToBlock.Count();

        _mockStatusCheckService.Verify(s => s.EnsureCorrectStatus(It.IsAny<BlockUriSettings>()), Times.Exactly(blockedUriCount));
    }

    [Fact]
    public async Task StopAsync_WhenSiteIsRevoked_GrantsAccess_And_FlushesCache()
    {
        _mockHostsFileService.Setup(hf => hf.IsRevoked(It.IsAny<string>())).Returns(true);

        var blockerService = new BlockerService(_mockSettings, _mockHostsFileService.Object, _mockCacheFlushService.Object,
            _mockLogger.Object, _mockStatusCheckService.Object);

        var cts = new CancellationTokenSource();

        await blockerService.StopAsync(cts.Token);

        var blockedUriCount = _mockSettings.Value.UrisToBlock.Count();

        _mockHostsFileService.Verify(hf => hf.IsRevoked(It.IsAny<string>()), Times.Exactly(blockedUriCount));
        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Exactly(blockedUriCount));
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Never());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Exactly(blockedUriCount));
    }

    [Fact]
    public async Task StopAsync_WhenSiteIsNotRevoked_DoesNotModifyHostsFile_And_DoesNotFlushCache()
    {
        _mockHostsFileService.Setup(hf => hf.IsRevoked(It.IsAny<string>())).Returns(false);

        var blockerService = new BlockerService(_mockSettings, _mockHostsFileService.Object, _mockCacheFlushService.Object,
            _mockLogger.Object, _mockStatusCheckService.Object);

        var cts = new CancellationTokenSource();

        await blockerService.StopAsync(cts.Token);

        var blockedUriCount = _mockSettings.Value.UrisToBlock.Count();

        _mockHostsFileService.Verify(hf => hf.IsRevoked(It.IsAny<string>()), Times.Exactly(blockedUriCount));
        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Never());
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Never());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Never());
    }
}
