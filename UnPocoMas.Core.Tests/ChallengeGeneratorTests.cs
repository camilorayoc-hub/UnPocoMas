using UnPocoMas.Core.Models;
using UnPocoMas.Core.Randomness;
using UnPocoMas.Core.Services;
using UnPocoMas.Core.Utils;

namespace UnPocoMas.Core.Tests;

public sealed class ChallengeGeneratorTests
{
    [Fact]
    public void Generate_IsDeterministicFor365ConsecutiveDates()
    {
        var generator = new ChallengeGenerator();
        DateOnly start = new(2026, 1, 1);

        for (int day = 0; day < 365; day++)
        {
            DateOnly date = start.AddDays(day);
            DailyChallenge first = generator.Generate(date);
            DailyChallenge second = generator.Generate(date);

            Assert.Equal(first.UtcDate, second.UtcDate);
            Assert.Equal(first.ChallengeNumber, second.ChallengeNumber);
            Assert.Equal(first.Seed, second.Seed);
            Assert.Equal(5, first.Rounds.Count);

            for (int round = 0; round < first.Rounds.Count; round++)
            {
                AssertRoundEqual(first.Rounds[round], second.Rounds[round]);
            }
        }
    }

    [Fact]
    public void Generate_UsesRequiredProfiles()
    {
        DailyChallenge challenge =
            new ChallengeGenerator().Generate(new DateOnly(2026, 1, 1));

        Assert.Equal(RoundProfile.Standard, challenge.Rounds[0].Profile);
        Assert.Equal(RoundProfile.Final, challenge.Rounds[4].Profile);

        RoundProfile[] middleProfiles = challenge.Rounds
            .Skip(1)
            .Take(3)
            .Select(round => round.Profile)
            .ToArray();

        Assert.Equal(3, middleProfiles.Distinct().Count());
    }

    [Fact]
    public void CrashMultiplier_HasExpectedDistribution()
    {
        const int sampleCount = 1_000_000;
        var random = new Mulberry32(0x12345678);
        int[] thresholds = { 2, 5, 10, 20 };
        int[] counts = new int[thresholds.Length];

        for (int sample = 0; sample < sampleCount; sample++)
        {
            double crashMultiplier =
                MultiplierCurves.GetCrashMultiplier(random.NextDouble());

            for (int threshold = 0; threshold < thresholds.Length; threshold++)
            {
                if (crashMultiplier >= thresholds[threshold])
                {
                    counts[threshold]++;
                }
            }
        }

        double[] expected = { 0.65, 0.26, 0.13, 0.065 };

        for (int index = 0; index < expected.Length; index++)
        {
            double actual = (double)counts[index] / sampleCount;
            Assert.InRange(actual, expected[index] - 0.005, expected[index] + 0.005);
        }
    }

    private static void AssertRoundEqual(RoundData expected, RoundData actual)
    {
        Assert.Equal(expected.RoundNumber, actual.RoundNumber);
        Assert.Equal(expected.Profile, actual.Profile);
        Assert.Equal(expected.CrashMultiplier, actual.CrashMultiplier);
        Assert.Equal(expected.CrashTimeSeconds, actual.CrashTimeSeconds);
        Assert.Equal(expected.HasRealSignal, actual.HasRealSignal);
        Assert.Equal(expected.RealSignalTimeSeconds, actual.RealSignalTimeSeconds);
        Assert.Equal(expected.HasFalseSignal, actual.HasFalseSignal);
        Assert.Equal(expected.FalseSignalTimeSeconds, actual.FalseSignalTimeSeconds);
        Assert.Equal(expected.FalseSignalDurationSeconds, actual.FalseSignalDurationSeconds);
        Assert.Equal(expected.BasePoints, actual.BasePoints);
    }
}
