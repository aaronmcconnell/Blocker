using Blocker.Jobs;
using Quartz;
using Quartz.Impl.Calendar;
using Blocker.Settings;
using Quartz.Impl.AdoJobStore;

namespace Blocker.DependencyInjection;

public static class QuartzExtensions
{
    public static IServiceCollectionQuartzConfigurator AddJobs(this IServiceCollectionQuartzConfigurator config, BlockerServiceSettings settings)
    {
        var uris = settings.UrisToBlock.Select(u => u);
        var blockUriSettingsEqualityComparer = new BlockUriSettingsUriComparer();
        var distincted = uris.Distinct(blockUriSettingsEqualityComparer);

        if (distincted.Count() < uris.Count())
            throw new InvalidConfigurationException("Duplicate configurations exist for the same Uri");

        foreach (var uriToBlock in settings.UrisToBlock)
        {
            var data = new JobDataMap() { { "uri", uriToBlock.UriToBlock } };

            var today = DateOnly.FromDateTime(DateTime.Now);

            var activeDays = uriToBlock.ActiveDays.Select(Enum.Parse<DayOfWeek>);
            var inactiveDays = Enum.GetValues<DayOfWeek>().Except(activeDays);

            var calendarName = $"weekday-calendar-{uriToBlock.UriToBlock}";

            config.AddCalendar<WeeklyCalendar>(calendarName, false, false, (cal) =>
            {
                cal.DaysExcluded = GetInactiveDaysArray(inactiveDays);
            });

            config.ScheduleJob<RevokeJob>(trigger =>
            {
                var time = settings.ReadTimeFromSetting(uriToBlock.BlockFrom);
                var startTime = new DateTimeOffset(today, time, DateTimeOffset.Now.Offset);
                trigger.StartAt(startTime);
                trigger.ModifiedByCalendar(calendarName);
                trigger.WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever());
                trigger.WithIdentity($"revoke-{uriToBlock.UriToBlock}");
                trigger.UsingJobData(data);
            });

            config.ScheduleJob<GrantJob>(trigger =>
            {
                var time = settings.ReadTimeFromSetting(uriToBlock.BlockTo);
                var startTime = new DateTimeOffset(today, time, DateTimeOffset.Now.Offset);
                trigger.StartAt(startTime);
                trigger.ModifiedByCalendar(calendarName);
                trigger.WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever());
                trigger.WithIdentity($"grant-{uriToBlock.UriToBlock}");
                trigger.UsingJobData(data);
            });

            config.ScheduleJob<CheckJob>(trigger =>
            {
                trigger.StartAt(DateTimeOffset.Now.AddMinutes(5));
                trigger.ModifiedByCalendar(calendarName);
                trigger.WithSimpleSchedule(s => s.WithIntervalInMinutes(5).RepeatForever());
                trigger.WithIdentity($"check-{uriToBlock.UriToBlock}");
                trigger.UsingJobData(data);
            });
        }

        return config;
    }

    private static bool[] GetInactiveDaysArray(IEnumerable<DayOfWeek> inactiveDays)
    {
        var array = new bool[8];
        var i = 0;
        foreach (var inactiveDay in inactiveDays)
        {
            i = (int)inactiveDay;
            array[i] = true;
        }
        return array;
    }
}
