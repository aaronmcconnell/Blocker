using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Blocker.Settings;

public class BlockerServiceSettings
{
    public static string SectionName => "BlockerService";

    [Required]
    public string HostsFilePath { get; set; } = null!;

    [Required]
    [ValidateEnumeratedItems]
    public IEnumerable<BlockUriSettings> UrisToBlock { get; set; } = null!;
    
    public TimeOnly ReadTimeFromSetting(string timeSetting)
    {
        var elements = timeSetting.Split(":".ToCharArray());
        var hr = Convert.ToInt32(elements[0]);
        var min = Convert.ToInt32(elements[1]);
        return new TimeOnly(hr, min);
    }
}
