namespace UnPocoMas.Core.Models;

public sealed class DailyResult
{
    public int ChallengeNumber { get; init; }

    public DateOnly Date { get; init; }

    public IReadOnlyList<RoundResult> Rounds { get; init; }
        = Array.Empty<RoundResult>();

    public int TotalPoints { get; init; }

    public double EfficiencyPercent { get; init; }

    public static DailyResult Create(
        DailyChallenge challenge,
        IEnumerable<RoundResult> rounds)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(rounds);

        RoundResult[] materializedRounds = rounds.ToArray();
        double successfulRetireMultipliers = 0.0;
        double crashMultipliers = 0.0;
        int totalPoints = 0;

        foreach (RoundResult round in materializedRounds)
        {
            crashMultipliers += round.CrashMultiplier;
            totalPoints += round.Points;

            if (round.Success)
            {
                successfulRetireMultipliers += round.RetireMultiplier;
            }
        }

        double efficiencyPercent = crashMultipliers == 0.0
            ? 0.0
            : successfulRetireMultipliers / crashMultipliers * 100.0;

        return new DailyResult
        {
            ChallengeNumber = challenge.ChallengeNumber,
            Date = challenge.UtcDate,
            Rounds = materializedRounds,
            TotalPoints = totalPoints,
            EfficiencyPercent = efficiencyPercent
        };
    }
}
