using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.UI.Services;

namespace UnPocoMas.Core.Tests;

public sealed class RoundRuntimeServiceTests
{
    [Fact]
    public async Task CurrentMultiplier_DoesNotExceedCrashMultiplier()
    {
        RoundData round = new()
        {
            RoundNumber = 1,
            Profile = RoundProfile.Standard,
            CrashMultiplier = 1.0,
            CrashTimeSeconds = 0.1,
            BasePoints = 100
        };
        var completion = new TaskCompletionSource<RoundResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        runtime.RoundCompleted += completion.SetResult;

        await runtime.StartAsync(round);
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(runtime.CurrentMultiplier <= round.CrashMultiplier);
    }
}