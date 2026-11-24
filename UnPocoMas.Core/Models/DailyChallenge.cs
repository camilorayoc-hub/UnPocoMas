namespace UnPocoMas.Core.Models;

public sealed class DailyChallenge
{
    public DateOnly UtcDate { get; init; }

    public int ChallengeNumber { get; init; }

    public uint Seed { get; init; }

    public IReadOnlyList<RoundData> Rounds { get; init; }
        = Array.Empty<RoundData>();
}
