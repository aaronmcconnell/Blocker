using System.ComponentModel.DataAnnotations;

namespace Blocker.Settings;

public class BlockUriSettings
{
    [Required]
    public string[] ActiveDays { get; set; } = null!;

    [Required]
    public string UriToBlock { get; set; } = null!;

    [Required]
    public string BlockFrom { get; set; } = null!;

    [Required]
    public string BlockTo { get; set; } = null!;
}