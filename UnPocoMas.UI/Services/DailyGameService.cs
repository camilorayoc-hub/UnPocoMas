using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.UI.Interfaces;
using UnPocoMas.UI.Models;

namespace UnPocoMas.UI.Services;

public sealed class DailyGameService
{
    private readonly ChallengeGenerator _challengeGenerator;
    private readonly ScoreCalculator _scoreCalculator;
    private readonly IStorageService _storage;
    private readonly IUtcDateProvider _utcDateProvider;
    private readonly ActiveChallengeState _state = new();
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly SemaphoreSlim _stateGate = new(1, 1);

    private DateOnly _utcDate;
    private bool _initialized;

    public DailyGameService(
        ChallengeGenerator challengeGenerator,
        ScoreCalculator scoreCalculator,
        IStorageService storage,
        IUtcDateProvider utcDateProvider)
    {
        _challengeGenerator = challengeGenerator;
        _scoreCalculator = scoreCalculator;
        _storage = storage;
        _utcDateProvider = utcDateProvider;
    }

    public bool IsInitialized => _initialized;

    public DailyChallenge CurrentChallenge =>
        _state.Challenge
        ?? throw new InvalidOperationException("No hay un reto activo.");

    public ActiveChallengeState State => _state;

    public SettingsModel Settings { get; private set; } = new();

    public StatisticsModel Statistics { get; private set; } = new();

    public RoundData CurrentRound =>
        CurrentChallenge.Rounds[_state.CurrentRoundIndex];

    public async Task InitializeAsync()
    {
        await _initializationGate.WaitAsync();

        try
        {
            DateOnly currentUtcDate = _utcDateProvider.TodayUtc;

            if (_initialized && _utcDate == currentUtcDate)
            {
                return;
            }

            _utcDate = currentUtcDate;
            Settings = await _storage.LoadAsync<SettingsModel>(
                StorageKeys.Settings) ?? new SettingsModel();
            Statistics = await _storage.LoadAsync<StatisticsModel>(
                StorageKeys.Statistics) ?? new StatisticsModel();

            DailyChallenge challenge = _challengeGenerator.Generate(_utcDate);
            DailyResult? dailyResult = await _storage.LoadAsync<DailyResult>(
                StorageKeys.DailyResult(_utcDate));

            if (IsValidDailyResult(dailyResult, challenge))
            {
                RestoreCompletedChallenge(challenge, dailyResult!);
                await UpdateStatisticsAsync(dailyResult!);
            }
            else
            {
                PersistedActiveChallenge? activeChallenge =
                    await _storage.LoadAsync<PersistedActiveChallenge>(
                        StorageKeys.ActiveChallenge);

                if (IsValidActiveChallenge(activeChallenge, challenge))
                {
                    RestoreActiveChallenge(challenge, activeChallenge!);
                }
                else
                {
                    ResetState(challenge);
                }
            }

            _initialized = true;
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    public async Task<RoundResult> AdvanceRoundAsync(
        double playerTimeSeconds)
    {
        EnsureInitialized();

        RoundResult result = _scoreCalculator.Calculate(
            CurrentRound,
            playerTimeSeconds);
        await RecordResultAsync(result);
        return result;
    }

    public async Task RecordResultAsync(RoundResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        EnsureInitialized();

        await _stateGate.WaitAsync();

        try
        {
            if (_state.IsCompleted)
            {
                throw new InvalidOperationException(
                    "El reto ya está completado.");
            }

            if (result.RoundNumber != CurrentRound.RoundNumber)
            {
                throw new InvalidOperationException(
                    "El resultado no corresponde a la ronda actual.");
            }

            _state.Results.Add(result);

            if (_state.CurrentRoundIndex >= CurrentChallenge.Rounds.Count - 1)
            {
                _state.IsCompleted = true;
                _state.CurrentRoundIndex = CurrentChallenge.Rounds.Count;

                DailyResult dailyResult = CreateDailyResult();
                await _storage.SaveAsync(
                    StorageKeys.DailyResult(_utcDate),
                    dailyResult);
                await UpdateStatisticsAsync(dailyResult);
                await _storage.RemoveAsync(StorageKeys.ActiveChallenge);
            }
            else
            {
                _state.CurrentRoundIndex++;
                await SaveActiveChallengeAsync();
            }
        }
        finally
        {
            _stateGate.Release();
        }
    }

    public async Task SaveSettingsAsync(SettingsModel settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;
        await _storage.SaveAsync(StorageKeys.Settings, settings);
    }

    public DailyResult CreateDailyResult()
    {
        EnsureInitialized();
        return DailyResult.Create(CurrentChallenge, _state.Results);
    }

    private async Task SaveActiveChallengeAsync()
    {
        var activeChallenge = new PersistedActiveChallenge
        {
            UtcDate = _utcDate,
            ChallengeNumber = CurrentChallenge.ChallengeNumber,
            CurrentRoundIndex = _state.CurrentRoundIndex,
            Results = _state.Results.ToList(),
            IsCompleted = false
        };

        await _storage.SaveAsync(
            StorageKeys.ActiveChallenge,
            activeChallenge);
    }

    private async Task UpdateStatisticsAsync(DailyResult dailyResult)
    {
        if (Statistics.LastPlayedUtcDate == _utcDate)
        {
            return;
        }

        int previousDaysPlayed = Statistics.DaysPlayed;
        DateOnly? previousDate = Statistics.LastPlayedUtcDate;

        Statistics.CurrentStreak = previousDate == _utcDate.AddDays(-1)
            ? Statistics.CurrentStreak + 1
            : 1;
        Statistics.DaysPlayed = previousDaysPlayed + 1;
        Statistics.BestScore = Math.Max(
            Statistics.BestScore,
            dailyResult.TotalPoints);
        Statistics.AverageEfficiency = previousDaysPlayed == 0
            ? dailyResult.EfficiencyPercent
            : (Statistics.AverageEfficiency * previousDaysPlayed
                + dailyResult.EfficiencyPercent)
                / Statistics.DaysPlayed;
        Statistics.LastPlayedUtcDate = _utcDate;

        await _storage.SaveAsync(StorageKeys.Statistics, Statistics);
    }

    private void RestoreCompletedChallenge(
        DailyChallenge challenge,
        DailyResult dailyResult)
    {
        _state.Challenge = challenge;
        _state.Results.Clear();
        _state.Results.AddRange(dailyResult.Rounds);
        _state.CurrentRoundIndex = challenge.Rounds.Count;
        _state.IsCompleted = true;
    }

    private void RestoreActiveChallenge(
        DailyChallenge challenge,
        PersistedActiveChallenge activeChallenge)
    {
        _state.Challenge = challenge;
        _state.Results.Clear();
        _state.Results.AddRange(activeChallenge.Results);
        _state.CurrentRoundIndex = activeChallenge.CurrentRoundIndex;
        _state.IsCompleted = false;
    }

    private void ResetState(DailyChallenge challenge)
    {
        _state.Challenge = challenge;
        _state.Results.Clear();
        _state.CurrentRoundIndex = 0;
        _state.IsCompleted = false;
    }

    private bool IsValidDailyResult(
        DailyResult? dailyResult,
        DailyChallenge challenge)
    {
        if (dailyResult is null
            || dailyResult.Date != _utcDate
            || dailyResult.ChallengeNumber != challenge.ChallengeNumber
            || dailyResult.Rounds.Count != challenge.Rounds.Count)
        {
            return false;
        }

        return dailyResult.Rounds.All(IsValidRoundResult);
    }

    private bool IsValidActiveChallenge(
        PersistedActiveChallenge? activeChallenge,
        DailyChallenge challenge)
    {
        if (activeChallenge is null
            || activeChallenge.IsCompleted
            || activeChallenge.UtcDate != _utcDate
            || activeChallenge.ChallengeNumber != challenge.ChallengeNumber
            || activeChallenge.CurrentRoundIndex < 0
            || activeChallenge.CurrentRoundIndex > challenge.Rounds.Count - 1
            || activeChallenge.Results.Count != activeChallenge.CurrentRoundIndex)
        {
            return false;
        }

        return activeChallenge.Results.All(IsValidRoundResult);
    }

    private static bool IsValidRoundResult(RoundResult result)
    {
        return result.RoundNumber > 0
            && result.Points >= 0
            && double.IsFinite(result.RetireMultiplier)
            && double.IsFinite(result.CrashMultiplier)
            && double.IsFinite(result.CrashTimeSeconds);
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "DailyGameService no está inicializado.");
        }
    }

    private sealed class PersistedActiveChallenge
    {
        public DateOnly UtcDate { get; init; }

        public int ChallengeNumber { get; init; }

        public int CurrentRoundIndex { get; init; }

        public List<RoundResult> Results { get; init; } = new();

        public bool IsCompleted { get; init; }
    }
}
