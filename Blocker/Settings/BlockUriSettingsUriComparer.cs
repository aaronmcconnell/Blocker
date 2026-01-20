namespace Blocker.Settings;

public sealed class BlockUriSettingsUriComparer : IEqualityComparer<BlockUriSettings>
{
    public bool Equals(BlockUriSettings? x, BlockUriSettings? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.UriToBlock, y.UriToBlock, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(BlockUriSettings obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.UriToBlock);
    }
}
