using AutoFixture;
using AutoFixture.Kernel;
using Blocker.Settings;

namespace Blocker.Tests.AutoFixture;

public sealed class BlockUriSettingsSpecimenBuilder : ISpecimenBuilder
{
    private static readonly string[] DayNames = Enum.GetNames<DayOfWeek>();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not Type type || type != typeof(BlockUriSettings))
        {
            return new NoSpecimen();
        }

        var dayCount = (context.Create<byte>() % DayNames.Length) + 1;
        var activeDays = DayNames
            .OrderBy(_ => context.Create<int>())
            .Take(dayCount)
            .ToArray();

        return new BlockUriSettings
        {
            ActiveDays = activeDays,
            UriToBlock = $"example-{context.Create<int>()}.com",
            BlockFrom = "09:00",
            BlockTo = "17:00",
        };
    }
}
