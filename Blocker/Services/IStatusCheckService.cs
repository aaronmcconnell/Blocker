using Blocker.Settings;

namespace Blocker.Services;

public interface IStatusCheckService
{
    Task EnsureCorrectStatus(BlockUriSettings blockSettings);
}
