namespace UnPocoMas.UI.Services;

public interface IUtcDateProvider
{
    DateOnly TodayUtc { get; }
}

public sealed class SystemUtcDateProvider : IUtcDateProvider
{
    private const long TestingDateRange = 3_652_058L;

    public DateOnly TodayUtc =>
        DateOnly.FromDayNumber(
            (int)((DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute)
                % TestingDateRange));
}
