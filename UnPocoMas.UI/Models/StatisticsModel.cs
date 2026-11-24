namespace UnPocoMas.UI.Models;

public sealed class StatisticsModel
{
    public int DaysPlayed { get; set; }

    public int CurrentStreak { get; set; }

    public int BestScore { get; set; }

    public double AverageEfficiency { get; set; }

    public DateOnly? LastPlayedUtcDate { get; set; }
}
