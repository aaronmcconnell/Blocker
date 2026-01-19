namespace Blocker.Services;

public interface IHostsFileService
{
    public bool IsRevoked(string uri);

    public void Revoke(string uri);

    public void Grant(string uri);
}