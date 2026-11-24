using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.Core.Utils;

namespace UnPocoMas.Core.Tests;

public sealed class ScoreCalculatorTests
{
    [Fact]
    public void Calculate_AroundLatencyBoundary()
    {
        RoundData round = CreateRound(100, RoundProfile.Standard, 2.0);
        var calculator = new ScoreCalculator();

        Assert.True(calculator.Calculate(round, 1.999).Success);
        Assert.True(calculator.Calculate(round, 2.049).Success);
        Assert.False(calculator.Calculate(round, 2.050).Success);
        Assert.False(calculator.Calculate(round, 2.051).Success);
    }

    [Fact]
    public void CreateCrashResult_CreatesRedZeroPointResult()
    {
        RoundData round = new()
        {
            RoundNumber = 4,
            Profile = RoundProfile.Trap,
            CrashMultiplier = 3.25,
            CrashTimeSeconds = 4.5,
            BasePoints = 100
        };

        RoundResult result = new ScoreCalculator().CreateCrashResult(round);

        Assert.False(result.Success);
        Assert.Equal(0.0, result.RetireMultiplier);
        Assert.Equal(0, result.Points);
        Assert.Equal("🟥", result.ShareSymbol);
        Assert.Equal(round.RoundNumber, result.RoundNumber);
        Assert.Equal(round.Profile, result.Profile);
        Assert.Equal(round.CrashMultiplier, result.CrashMultiplier);
        Assert.Equal(round.CrashTimeSeconds, result.CrashTimeSeconds);
    }

    [Fact]
    public void Calculate_UsesConfiguredBasePointsAndRounding()
    {
        double crashTime = Math.Log(1.006) / 0.25;
        RoundData normalRound = CreateRound(100, RoundProfile.Standard, crashTime);
        RoundData finalRound = CreateRound(200, RoundProfile.Final, crashTime);
        var calculator = new ScoreCalculator();

        RoundResult normalResult = calculator.Calculate(normalRound, crashTime);
        RoundResult finalResult = calculator.Calculate(finalRound, crashTime);

        Assert.Equal(101, normalResult.Points);
        Assert.Equal(201, finalResult.Points);
    }

    [Fact]
    public void Calculate_AssignsExpectedShareSymbols()
    {
        RoundData round = CreateRound(100, RoundProfile.Standard, 2.0);
        var calculator = new ScoreCalculator();

        RoundResult failure = calculator.Calculate(round, 2.051);
        RoundResult normalSuccess = calculator.Calculate(round, 0.0);
        RoundResult highSuccess = calculator.Calculate(round, 2.0);

        Assert.Equal("🟥", failure.ShareSymbol);
        Assert.Equal("🟩", normalSuccess.ShareSymbol);
        Assert.Equal("🟨", highSuccess.ShareSymbol);
    }

    private static RoundData CreateRound(
        int basePoints,
        RoundProfile profile,
        double crashTimeSeconds)
    {
        return new RoundData
        {
            RoundNumber = 1,
            Profile = profile,
            CrashMultiplier = MultiplierCurves.GetMultiplier(
                profile,
                crashTimeSeconds),
            CrashTimeSeconds = crashTimeSeconds,
            BasePoints = basePoints
        };
    }
}
