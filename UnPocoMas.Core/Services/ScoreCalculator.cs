using UnPocoMas.Core.Configuration;
using UnPocoMas.Core.Models;
using UnPocoMas.Core.Utils;

namespace UnPocoMas.Core.Services;

public sealed class ScoreCalculator
{
    public RoundResult Calculate(
        RoundData round,
        double playerTimeSeconds)
    {
        ArgumentNullException.ThrowIfNull(round);

        double crashTimeSeconds = round.CrashTimeSeconds;
        bool success =
            playerTimeSeconds <
            crashTimeSeconds + GameConfig.SuccessLatencySeconds;

        if (!success)
        {
            return CreateCrashResult(round);
        }

        double effectiveTime =
            Math.Min(playerTimeSeconds, crashTimeSeconds);
        double retireMultiplier =
            MultiplierCurves.GetMultiplier(round.Profile, effectiveTime);
        int points = (int)Math.Round(
            retireMultiplier * round.BasePoints,
            MidpointRounding.AwayFromZero);
        string shareSymbol = retireMultiplier >=
            0.90 * round.CrashMultiplier
            ? "🟨"
            : "🟩";

        return new RoundResult
        {
            RoundNumber = round.RoundNumber,
            Success = true,
            RetireMultiplier = retireMultiplier,
            Points = points,
            CrashMultiplier = round.CrashMultiplier,
            CrashTimeSeconds = crashTimeSeconds,
            Profile = round.Profile,
            ShareSymbol = shareSymbol
        };
    }

    public RoundResult CreateCrashResult(RoundData round)
    {
        ArgumentNullException.ThrowIfNull(round);

        return new RoundResult
        {
            RoundNumber = round.RoundNumber,
            Success = false,
            RetireMultiplier = 0.0,
            Points = 0,
            CrashMultiplier = round.CrashMultiplier,
            CrashTimeSeconds = round.CrashTimeSeconds,
            Profile = round.Profile,
            ShareSymbol = "🟥"
        };
    }
}
