using System.Globalization;

namespace UnPocoMas.UI;

public static class StorageKeys
{
    public const string Settings = "upm.v1.settings";

    public const string Statistics = "upm.v1.statistics";

    public const string ActiveChallenge = "upm.v1.active-challenge";

    public static string DailyResult(DateOnly utcDate)
    {
        return $"upm.v1.daily-result.{utcDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
    }
}
