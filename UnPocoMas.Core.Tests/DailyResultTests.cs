using System.Globalization;
using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;

namespace UnPocoMas.Core.Tests;

public sealed class DailyResultTests
{
    [Fact]
    public void Create_CalculatesTotalsAndEfficiency()
    {
        var challenge = new DailyChallenge
        {
            UtcDate = new DateOnly(2026, 1, 1),
            ChallengeNumber = 142
        };
        RoundResult[] rounds =
        {
            new() { Success = true, RetireMultiplier = 2.0, Points = 200, CrashMultiplier = 2.5 },
            new() { Success = false, RetireMultiplier = 0.0, Points = 0, CrashMultiplier = 3.0 }
        };

        DailyResult result = DailyResult.Create(challenge, rounds);

        Assert.Equal(142, result.ChallengeNumber);
        Assert.Equal(challenge.UtcDate, result.Date);
        Assert.Equal(200, result.TotalPoints);
        Assert.Equal(2.0 / 5.5 * 100.0, result.EfficiencyPercent);
    }

    [Fact]
    public void Build_UsesRequiredShareFormat()
    {
        var result = new DailyResult
        {
            ChallengeNumber = 142,
            TotalPoints = 3840,
            EfficiencyPercent = 41.0,
            Rounds = new RoundResult[]
            {
                new() { ShareSymbol = "🟩" },
                new() { ShareSymbol = "🟩" },
                new() { ShareSymbol = "🟥" },
                new() { ShareSymbol = "🟩" },
                new() { ShareSymbol = "🟨" }
            }
        };

        string expected = string.Join(
            Environment.NewLine,
            "Un Poco Más #142",
            string.Empty,
            "🟩🟩🟥🟩🟨  38.4x",
            string.Empty,
            "Eficiencia 41 %");

        Assert.Equal(expected, new ShareTextBuilder().Build(result));
        Assert.DoesNotContain("Crash", expected, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Profile", expected, StringComparison.OrdinalIgnoreCase);
        Assert.True(double.TryParse(
            result.EfficiencyPercent.ToString(CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture,
            out _));
    }
}
