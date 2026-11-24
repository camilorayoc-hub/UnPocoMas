using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.UI.Services;

namespace UnPocoMas.Core.Tests;

public sealed class RoundRuntimeLifecycleTests
{
    [Fact]
    public async Task BackgroundedActiveRoundProducesOneCrashResult()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        RoundData round = CreateRound();
        var results = new List<RoundResult>();
        runtime.RoundCompleted += results.Add;

        await runtime.StartAsync(round);
        runtime.HandleBackgrounded();

        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.Equal(0, results[0].Points);
        Assert.Equal(0, results[0].RetireMultiplier);
        Assert.Equal("🟥", results[0].ShareSymbol);
    }

    [Fact]
    public async Task TwoBackgroundNotificationsDoNotDuplicateResult()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        var results = new List<RoundResult>();
        runtime.RoundCompleted += results.Add;

        await runtime.StartAsync(CreateRound());
        runtime.HandleBackgrounded();
        runtime.HandleBackgrounded();

        Assert.Single(results);
    }

    [Fact]
    public async Task BackgroundedAfterCompletionDoesNothing()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        var results = new List<RoundResult>();
        runtime.RoundCompleted += results.Add;

        await runtime.StartAsync(CreateRound());
        await runtime.RetireAsync();
        runtime.HandleBackgrounded();

        Assert.Single(results);
    }

    [Fact]
    public async Task BackgroundedDuringCountdownCancelsWithoutResultOrAdvance()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        RoundData round = CreateRound();
        var countdownStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var results = new List<RoundResult>();
        runtime.RoundCompleted += results.Add;
        runtime.StateChanged += () =>
        {
            if (runtime.IsCountdown)
            {
                countdownStarted.TrySetResult(true);
            }
        };

        Task startTask = runtime.StartAsync(round);
        await countdownStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        runtime.HandleBackgrounded();
        await startTask;

        Assert.Empty(results);
        Assert.False(runtime.IsRunning);
        Assert.False(runtime.IsFinished);
        Assert.True(runtime.WasCountdownCancelledByBackground());
        Assert.Same(round, runtime.CurrentRound);
    }

    [Fact]
    public async Task CancelledCountdownCanRestartWithTheSameRound()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        RoundData round = CreateRound();
        var countdownStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        runtime.StateChanged += () =>
        {
            if (runtime.IsCountdown)
            {
                countdownStarted.TrySetResult(true);
            }
        };

        Task cancelledStart = runtime.StartAsync(round);
        await countdownStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        runtime.HandleBackgrounded();
        await cancelledStart;

        Task restarted = runtime.StartAsync(runtime.CurrentRound);
        await restarted;

        Assert.Same(round, runtime.CurrentRound);
        Assert.True(runtime.IsRunning);
        runtime.HandleBackgrounded();
    }

    [Fact]
    public async Task RetirementAndBackgroundProduceOnlyOneCompletion()
    {
        using var runtime = new RoundRuntimeService(new ScoreCalculator());
        var results = new List<RoundResult>();
        runtime.RoundCompleted += results.Add;

        await runtime.StartAsync(CreateRound());
        Task retire = runtime.RetireAsync();
        runtime.HandleBackgrounded();
        await retire;

        Assert.Single(results);
    }

    private static RoundData CreateRound()
    {
        return new RoundData
        {
            RoundNumber = 1,
            Profile = RoundProfile.Standard,
            CrashMultiplier = 2.0,
            CrashTimeSeconds = 30.0,
            BasePoints = 100
        };
    }
}
