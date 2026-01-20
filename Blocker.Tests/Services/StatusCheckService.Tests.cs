using AutoFixture;
using Blocker.Services;
using Blocker.Settings;
using Blocker.Tests.AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using NodaTime.Testing;

namespace Blocker.Tests.Services;

public class StatusCheckServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IHostsFileService> _mockHostsFileService;
    private readonly Mock<ICacheFlushService> _mockCacheFlushService;
    private readonly Mock<ILogger<StatusCheckService>> _mockLogger;
    private readonly IOptions<BlockerServiceSettings> _mockSettings;
    private readonly FakeClock _fakeClock;

    public StatusCheckServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new BlockUriSettingsSpecimenBuilder());
        _mockCacheFlushService = new Mock<ICacheFlushService>();
        _mockHostsFileService = new Mock<IHostsFileService>();
        _mockLogger = new Mock<ILogger<StatusCheckService>>();
        _mockSettings = Options.Create(_fixture.Create<BlockerServiceSettings>());
        _fakeClock = new FakeClock(Instant.FromUtc(2026, 1, 19, 12, 0, 0)); // Monday
    }

    [Fact]
    public async Task EnsureCorrectStatus_WhenShouldBeRevoked_And_IsRevoked_NoActionTaken()
    {
        var uriBlockSetting = _mockSettings.Value.UrisToBlock.First();
        var siteToBlock = uriBlockSetting.UriToBlock;
        uriBlockSetting.ActiveDays = ["Monday"];
        uriBlockSetting.BlockFrom = "11:00";
        uriBlockSetting.BlockTo = "13:00";
                    
        _mockHostsFileService.Setup(hf => hf.IsRevoked(siteToBlock)).Returns(true);

        var service = new StatusCheckService(_mockHostsFileService.Object,_mockCacheFlushService.Object, 
            _mockSettings, _mockLogger.Object, _fakeClock);
        
        await service.EnsureCorrectStatus(uriBlockSetting);

        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Never());
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Never());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Never());
    }

    [Fact]
    public async Task EnsureCorrectStatus_WhenShouldBeRevoked_And_IsNotRevoked_CallsRevoke()
    {
        var uriBlockSetting = _mockSettings.Value.UrisToBlock.First();
        var siteToBlock = uriBlockSetting.UriToBlock;
        uriBlockSetting.ActiveDays = ["Monday"];
        uriBlockSetting.BlockFrom = "11:00";
        uriBlockSetting.BlockTo = "13:00";

        _mockHostsFileService.Setup(hf => hf.IsRevoked(siteToBlock)).Returns(false);

        var service = new StatusCheckService(_mockHostsFileService.Object,_mockCacheFlushService.Object, 
            _mockSettings, _mockLogger.Object, _fakeClock);
        
        await service.EnsureCorrectStatus(_mockSettings.Value.UrisToBlock.First());

        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Never());
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Once());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Once());
    }

    [Fact]
    public async Task EnsureCorrectStatus_WhenShouldNotBeRevoked_And_IsNotRevoked_NoActionTaken()
    {
        var uriBlockSetting = _mockSettings.Value.UrisToBlock.First();
        var siteToBlock = uriBlockSetting.UriToBlock;
        uriBlockSetting.ActiveDays = ["Tuesday"];
        uriBlockSetting.BlockFrom = "11:00";
        uriBlockSetting.BlockTo = "13:00";
                    
        _mockHostsFileService.Setup(hf => hf.IsRevoked(siteToBlock)).Returns(false);

        var service = new StatusCheckService(_mockHostsFileService.Object,_mockCacheFlushService.Object, 
            _mockSettings, _mockLogger.Object, _fakeClock);
        
        await service.EnsureCorrectStatus(uriBlockSetting);

        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Never());
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Never());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Never());
    }

    [Fact]
    public async Task EnsureCorrectStatus_WhenShouldNotBeRevoked_And_IsRevoked_CallsGrant()
    {
        var uriBlockSetting = _mockSettings.Value.UrisToBlock.First();
        var siteToBlock = uriBlockSetting.UriToBlock;
        uriBlockSetting.ActiveDays = ["Tuesday"];
        uriBlockSetting.BlockFrom = "11:00";
        uriBlockSetting.BlockTo = "13:00";

        _mockHostsFileService.Setup(hf => hf.IsRevoked(siteToBlock)).Returns(true);

        var service = new StatusCheckService(_mockHostsFileService.Object,_mockCacheFlushService.Object, 
            _mockSettings, _mockLogger.Object, _fakeClock);
        
        await service.EnsureCorrectStatus(_mockSettings.Value.UrisToBlock.First());

        _mockHostsFileService.Verify(hf => hf.Grant(It.IsAny<string>()), Times.Once());
        _mockHostsFileService.Verify(hf => hf.Revoke(It.IsAny<string>()), Times.Never());
        _mockCacheFlushService.Verify(cf => cf.Flush(), Times.Once());
    }
}
