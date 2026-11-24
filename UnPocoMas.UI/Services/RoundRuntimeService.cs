using System.Diagnostics;
using UnPocoMas.Core.Configuration;
using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.Core.Utils;

namespace UnPocoMas.UI.Services;

public sealed class RoundRuntimeService : IDisposable
{
    private static readonly TimeSpan UpdateInterval =
        TimeSpan.FromMilliseconds(30);

    private readonly ScoreCalculator _scoreCalculator;
    private readonly Stopwatch _stopwatch = new();
    private readonly object _sync = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _countdownCancelledByBackground;
    private bool _finished;
    private bool _disposed;

    public RoundRuntimeService(ScoreCalculator scoreCalculator)
    {
        _scoreCalculator = scoreCalculator;
    }

    public RoundData CurrentRound { get; private set; } = null!;

    public double CurrentMultiplier { get; private set; } = 1.0;

    public double ElapsedSeconds => _stopwatch.Elapsed.TotalSeconds;

    public bool IsRunning { get; private set; }

    public bool IsCountdown { get; private set; }

    public int CountdownValue { get; private set; }

    public bool IsFinished => _finished;

    public bool WasCountdownCancelledByBackground()
    {
        lock (_sync)
        {
            return _countdownCancelledByBackground;
        }
    }

    public event Action? StateChanged;

    public event Action<RoundResult>? RoundCompleted;

    public async Task StartAsync(RoundData round)
    {
        ArgumentNullException.ThrowIfNull(round);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_sync)
        {
            if (IsRunning || IsCountdown)
            {
                throw new InvalidOperationException(
                    "La ronda ya está en ejecución.");
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            CurrentRound = round;
            CurrentMultiplier = MultiplierCurves.GetMultiplier(
                round.Profile,
                0.0);
            CountdownValue = 3;
            IsCountdown = true;
            IsRunning = false;
            _countdownCancelledByBackground = false;
            _finished = false;
            _stopwatch.Reset();
        }

        NotifyStateChanged();

        try
        {
            for (int value = 3; value >= 1; value--)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    _cancellationTokenSource.Token);

                lock (_sync)
                {
                    CountdownValue = value - 1;
                }

                NotifyStateChanged();
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(250),
                _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            lock (_sync)
            {
                IsCountdown = false;
                CountdownValue = 0;
            }

            NotifyStateChanged();
            return;
        }

        lock (_sync)
        {
            if (_disposed || _finished)
            {
                return;
            }

            IsCountdown = false;
            IsRunning = true;
            _stopwatch.Restart();
        }

        NotifyStateChanged();
        _ = MonitorAsync(_cancellationTokenSource.Token);
    }

    public Task RetireAsync()
    {
        RoundResult? result = null;

        lock (_sync)
        {
            if (!IsRunning || _finished)
            {
                return Task.CompletedTask;
            }

            double elapsed = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Stop();
            IsRunning = false;
            _finished = true;

            result = _scoreCalculator.Calculate(CurrentRound, elapsed);
            _cancellationTokenSource?.Cancel();
        }

        NotifyStateChanged();
        NotifyCompleted(result);
        return Task.CompletedTask;
    }

    public void HandleBackgrounded()
    {
        RoundResult? result = null;

        lock (_sync)
        {
            if (_finished)
            {
                return;
            }

            if (IsCountdown)
            {
                IsCountdown = false;
                CountdownValue = 0;
                _countdownCancelledByBackground = true;
                _cancellationTokenSource?.Cancel();
            }
            else if (IsRunning)
            {
                _stopwatch.Stop();
                IsRunning = false;
                _finished = true;
                _cancellationTokenSource?.Cancel();
                result = _scoreCalculator.CreateCrashResult(CurrentRound);
            }
            else
            {
                return;
            }
        }

        NotifyStateChanged();

        if (result is not null)
        {
            NotifyCompleted(result);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _stopwatch.Stop();
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(UpdateInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                RoundResult? result = null;

                lock (_sync)
                {
                    if (!IsRunning || _finished)
                    {
                        return;
                    }

                    double elapsed = _stopwatch.Elapsed.TotalSeconds;
                    CurrentMultiplier = Math.Min(
                        MultiplierCurves.GetMultiplier(
                            CurrentRound.Profile,
                            elapsed),
                        CurrentRound.CrashMultiplier);

                    if (elapsed >= CurrentRound.CrashTimeSeconds
                        + GameConfig.SuccessLatencySeconds)
                    {
                        _stopwatch.Stop();
                        IsRunning = false;
                        _finished = true;
                        _cancellationTokenSource?.Cancel();

                        result = _scoreCalculator.CreateCrashResult(CurrentRound);
                    }
                }

                NotifyStateChanged();

                if (result is not null)
                {
                    NotifyCompleted(result);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private void NotifyCompleted(RoundResult result)
    {
        RoundCompleted?.Invoke(result);
    }
}
