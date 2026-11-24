using UnPocoMas.Core.Configuration;
using UnPocoMas.Core.Models;
using UnPocoMas.Core.Randomness;
using UnPocoMas.Core.Utils;

namespace UnPocoMas.Core.Services;

public sealed class ChallengeGenerator
{
    private const double FalseSignalDurationSeconds = 0.6;

    private static readonly RoundProfile[] SelectableProfiles =
    {
        RoundProfile.Standard,
        RoundProfile.Accelerated,
        RoundProfile.Lightning,
        RoundProfile.Trap
    };

    public DailyChallenge Generate(DateOnly utcDate)
    {
        uint seed = Fnv1a32.DailySeed(utcDate);
        var random = new Mulberry32(seed);
        RoundProfile[] middleProfiles = SelectMiddleProfiles(random);

        var rounds = new List<RoundData>(5)
        {
            GenerateRound(1, RoundProfile.Standard, random),
            GenerateRound(2, middleProfiles[0], random),
            GenerateRound(3, middleProfiles[1], random),
            GenerateRound(4, middleProfiles[2], random),
            GenerateRound(5, RoundProfile.Final, random)
        };

        int challengeNumber =
            GameConfig.FirstChallengeNumber
            + utcDate.DayNumber
            - GameConfig.LaunchDate.DayNumber;

        return new DailyChallenge
        {
            UtcDate = utcDate,
            ChallengeNumber = challengeNumber,
            Seed = seed,
            Rounds = rounds
        };
    }

    private static RoundProfile[] SelectMiddleProfiles(Mulberry32 random)
    {
        var profiles = (RoundProfile[])SelectableProfiles.Clone();

        for (int index = profiles.Length - 1; index > 0; index--)
        {
            int selectedIndex = (int)(random.NextDouble() * (index + 1));
            (profiles[index], profiles[selectedIndex]) =
                (profiles[selectedIndex], profiles[index]);
        }

        return new[] { profiles[0], profiles[1], profiles[2] };
    }

    private static RoundData GenerateRound(
        int roundNumber,
        RoundProfile profile,
        Mulberry32 random)
    {
        double crashRandom = random.NextDouble();
        double realSignalRandom = random.NextDouble();
        double anticipationRandom = random.NextDouble();
        double falseSignalRandom = random.NextDouble();
        double falseInstantRandom = random.NextDouble();

        double crashMultiplier =
            MultiplierCurves.GetCrashMultiplier(crashRandom);
        double crashTimeSeconds =
            MultiplierCurves.GetCrashTime(profile, crashMultiplier);

        bool hasRealSignal =
            realSignalRandom < GetRealSignalProbability(profile);
        double? realSignalTimeSeconds = null;

        if (hasRealSignal)
        {
            double leadTime =
                GetLeadTime(profile, anticipationRandom);
            realSignalTimeSeconds =
                Math.Max(0.0, crashTimeSeconds - leadTime);
        }

        bool hasFalseSignal = falseSignalRandom <
            GetFalseSignalProbability(profile);
        double? falseSignalTimeSeconds = null;

        if (hasFalseSignal)
        {
            double candidate =
                0.30 * crashTimeSeconds
                + falseInstantRandom * (0.60 * crashTimeSeconds);

            if (!hasRealSignal
                || !SignalsOverlap(
                    realSignalTimeSeconds!.Value,
                    candidate))
            {
                falseSignalTimeSeconds = candidate;
            }
            else
            {
                hasFalseSignal = false;
            }
        }

        return new RoundData
        {
            RoundNumber = roundNumber,
            Profile = profile,
            CrashMultiplier = crashMultiplier,
            CrashTimeSeconds = crashTimeSeconds,
            HasRealSignal = hasRealSignal,
            RealSignalTimeSeconds = realSignalTimeSeconds,
            HasFalseSignal = hasFalseSignal,
            FalseSignalTimeSeconds = falseSignalTimeSeconds,
            FalseSignalDurationSeconds = FalseSignalDurationSeconds,
            BasePoints = profile == RoundProfile.Final
                ? GameConfig.FinalRoundBasePoints
                : GameConfig.BasePoints
        };
    }

    private static double GetRealSignalProbability(RoundProfile profile)
    {
        return profile switch
        {
            RoundProfile.Standard => 0.75,
            RoundProfile.Accelerated => 0.75,
            RoundProfile.Lightning => 0.75,
            RoundProfile.Trap => 0.60,
            RoundProfile.Final => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
    }

    private static double GetLeadTime(
        RoundProfile profile,
        double anticipation)
    {
        return profile switch
        {
            RoundProfile.Standard or RoundProfile.Trap =>
                0.5 + anticipation * (1.2 - 0.5),
            RoundProfile.Accelerated =>
                0.4 + anticipation * (0.9 - 0.4),
            RoundProfile.Lightning =>
                0.2 + anticipation * (0.5 - 0.2),
            RoundProfile.Final => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
    }

    private static double GetFalseSignalProbability(RoundProfile profile)
    {
        return profile switch
        {
            RoundProfile.Standard => 0.10,
            RoundProfile.Accelerated => 0.10,
            RoundProfile.Lightning => 0.10,
            RoundProfile.Trap => 0.50,
            RoundProfile.Final => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
    }

    private static bool SignalsOverlap(
        double realStart,
        double falseStart)
    {
        double realEnd = realStart + FalseSignalDurationSeconds;
        double falseEnd = falseStart + FalseSignalDurationSeconds;

        return falseStart < realEnd && falseEnd > realStart;
    }
}
