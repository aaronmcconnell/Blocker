using Blocker.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Blocker.Extensions;

public static class IServiceCollectionExtensions
{
    public static void ValidateBlockerServiceSettings(this IServiceCollection serviceCollection, BlockerServiceSettings settings)
    {
        var exceptions = new List<Exception>();

        if (!File.Exists(settings.HostsFilePath))
            exceptions.Add(new FileNotFoundException("Host file not found"));

        if (settings.UrisToBlock == null)
            exceptions.Add(new ArgumentNullException(nameof(settings.UrisToBlock)));

        foreach (var blockSetting in settings.UrisToBlock ?? Array.Empty<BlockUriSettings>())
        {
            if (!IsValidHostOrUri(blockSetting.UriToBlock))
                exceptions.Add(new UriFormatException($"Not a valid host or Uri: {blockSetting.UriToBlock}"));

            if (blockSetting.ActiveDays == null || !blockSetting.ActiveDays.Any())
                exceptions.Add(new ArgumentException("ActiveDays is an empty array", nameof(blockSetting.ActiveDays)));

            foreach (var activeDay in blockSetting.ActiveDays ?? Array.Empty<string>())
            {
                if (!Enum.TryParse<DayOfWeek>(activeDay, true, out _))
                    exceptions.Add(new InvalidDataException($"{activeDay} is not a valid day of the week"));
            }

            var blockFromIsValid = TryParseTime(blockSetting.BlockFrom, out var tBlockFrom);
            if (!blockFromIsValid)
                exceptions.Add(new ArgumentException("Not a valid time", nameof(blockSetting.BlockFrom)));

            var blockToIsValid = TryParseTime(blockSetting.BlockTo, out var tBlockTo);
            if (!blockToIsValid)
                exceptions.Add(new ArgumentException("Not a valid time", nameof(blockSetting.BlockTo)));

            if (blockFromIsValid && blockToIsValid && tBlockFrom > tBlockTo)
                exceptions.Add(new ArgumentException("BlockTo must be after BlockFrom", nameof(blockSetting.BlockTo)));
        }

        if (exceptions.Count == 1) throw exceptions.Single();
        if (exceptions.Count > 1) throw new AggregateException(exceptions);
    }

    private static bool IsValidHostOrUri(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (Uri.CheckHostName(value) != UriHostNameType.Unknown)
            return true;

        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    private static bool TryParseTime(string value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
    }
}
