using System.Text.Json;
using UnPocoMas.Core.Models;
using UnPocoMas.Core.Services;
using UnPocoMas.UI;
using UnPocoMas.UI.Interfaces;
using UnPocoMas.UI.Models;
using UnPocoMas.UI.Services;

namespace UnPocoMas.Core.Tests;

public sealed class DailyGameServicePersistenceTests
{
    [Fact]
    public async Task TodaysResultBlocksAnotherAttemptAfterReinitialization()
    {
        var storage = new MemoryStorageService();
        DateOnly date = new(2026, 1, 1);

        await CompleteChallengeAsync(storage, date, 100, 50.0);

        var restored = CreateService(storage, date);
        await restored.InitializeAsync();

        Assert.True(restored.State.IsCompleted);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => restored.RecordResultAsync(restored.State.Results[0]));
    }

    [Fact]
    public async Task ResultFromAnotherDayDoesNotBlockCurrentChallenge()
    {
        var storage = new MemoryStorageService();
        DateOnly yesterday = new(2025, 12, 31);
        DateOnly today = new(2026, 1, 1);

        await storage.SaveAsync(
            StorageKeys.DailyResult(yesterday),
            new DailyResult
            {
                Date = yesterday,
                ChallengeNumber = 1,
                Rounds = Array.Empty<RoundResult>()
            });

        var service = CreateService(storage, today);
        await service.InitializeAsync();

        Assert.False(service.State.IsCompleted);
        Assert.Empty(service.State.Results);
        Assert.Equal(0, service.State.CurrentRoundIndex);
    }

    [Fact]
    public async Task PartialChallengeRestoresIndexAndResults()
    {
        var storage = new MemoryStorageService();
        DateOnly date = new(2026, 1, 1);
        var original = CreateService(storage, date);
        await original.InitializeAsync();

        RoundResult firstResult = CreateResult(original.CurrentRound, 100, 50.0);
        await original.RecordResultAsync(firstResult);

        var restored = CreateService(storage, date);
        await restored.InitializeAsync();

        Assert.False(restored.State.IsCompleted);
        Assert.Equal(1, restored.State.CurrentRoundIndex);
        Assert.Single(restored.State.Results);
        Assert.Equal(1, restored.State.Results[0].RoundNumber);
    }

    [Fact]
    public async Task CorruptActiveDataIsIgnoredSafely()
    {
        var storage = new MemoryStorageService();
        await storage.SetRawAsync(
            StorageKeys.ActiveChallenge,
            "{ not valid json }");

        var service = CreateService(storage, new DateOnly(2026, 1, 1));
        await service.InitializeAsync();

        Assert.False(service.State.IsCompleted);
        Assert.Equal(0, service.State.CurrentRoundIndex);
        Assert.Empty(service.State.Results);
    }

    [Fact]
    public async Task CompletingTwiceDoesNotDuplicateStatistics()
    {
        var storage = new MemoryStorageService();
        DateOnly date = new(2026, 1, 1);

        await CompleteChallengeAsync(storage, date, 100, 50.0);
        var secondInstance = CreateService(storage, date);
        await secondInstance.InitializeAsync();

        StatisticsModel statistics = await storage.LoadAsync<StatisticsModel>(
            StorageKeys.Statistics) ?? new StatisticsModel();

        Assert.Equal(1, statistics.DaysPlayed);
        Assert.Equal(1, statistics.CurrentStreak);
        Assert.Equal(50.0, statistics.AverageEfficiency);
        Assert.Equal(100, statistics.BestScore);
    }

    [Fact]
    public async Task StreakContinuesWhenLastPlayedDateWasYesterday()
    {
        var storage = new MemoryStorageService();
        DateOnly firstDate = new(2026, 1, 1);
        DateOnly secondDate = new(2026, 1, 2);

        await CompleteChallengeAsync(storage, firstDate, 100, 50.0);
        await CompleteChallengeAsync(storage, secondDate, 200, 100.0);

        StatisticsModel statistics = await storage.LoadAsync<StatisticsModel>(
            StorageKeys.Statistics) ?? new StatisticsModel();

        Assert.Equal(2, statistics.DaysPlayed);
        Assert.Equal(2, statistics.CurrentStreak);
        Assert.Equal(200, statistics.BestScore);
        Assert.Equal(75.0, statistics.AverageEfficiency);
    }

    [Fact]
    public async Task StreakResetsAfterAnUnplayedDay()
    {
        var storage = new MemoryStorageService();
        DateOnly firstDate = new(2026, 1, 1);
        DateOnly thirdDate = new(2026, 1, 3);

        await CompleteChallengeAsync(storage, firstDate, 100, 50.0);
        await CompleteChallengeAsync(storage, thirdDate, 100, 100.0);

        StatisticsModel statistics = await storage.LoadAsync<StatisticsModel>(
            StorageKeys.Statistics) ?? new StatisticsModel();

        Assert.Equal(2, statistics.DaysPlayed);
        Assert.Equal(1, statistics.CurrentStreak);
    }

    [Fact]
    public async Task BestScoreDoesNotDecrease()
    {
        var storage = new MemoryStorageService();

        await CompleteChallengeAsync(
            storage,
            new DateOnly(2026, 1, 1),
            200,
            50.0);
        await CompleteChallengeAsync(
            storage,
            new DateOnly(2026, 1, 2),
            100,
            50.0);

        StatisticsModel statistics = await storage.LoadAsync<StatisticsModel>(
            StorageKeys.Statistics) ?? new StatisticsModel();

        Assert.Equal(200, statistics.BestScore);
    }

    [Fact]
    public async Task AverageEfficiencyIsUpdatedAcrossOfficialDays()
    {
        var storage = new MemoryStorageService();

        await CompleteChallengeAsync(
            storage,
            new DateOnly(2026, 1, 1),
            100,
            25.0);
        await CompleteChallengeAsync(
            storage,
            new DateOnly(2026, 1, 2),
            100,
            75.0);

        StatisticsModel statistics = await storage.LoadAsync<StatisticsModel>(
            StorageKeys.Statistics) ?? new StatisticsModel();

        Assert.Equal(50.0, statistics.AverageEfficiency);
    }

    private static DailyGameService CreateService(
        MemoryStorageService storage,
        DateOnly date)
    {
        return new DailyGameService(
            new ChallengeGenerator(),
            new ScoreCalculator(),
            storage,
            new FixedUtcDateProvider(date));
    }

    private static async Task CompleteChallengeAsync(
        MemoryStorageService storage,
        DateOnly date,
        int points,
        double efficiency)
    {
        DailyGameService service = CreateService(storage, date);
        await service.InitializeAsync();

        for (int index = 0; index < 5; index++)
        {
            RoundData round = service.CurrentRound;
            await service.RecordResultAsync(
                CreateResult(round, points / 5, efficiency));
        }
    }

    private static RoundResult CreateResult(
        RoundData round,
        int points,
        double efficiency)
    {
        return new RoundResult
        {
            RoundNumber = round.RoundNumber,
            Success = true,
            RetireMultiplier = efficiency / 100.0,
            Points = points,
            CrashMultiplier = 1.0,
            CrashTimeSeconds = 1.0,
            Profile = round.Profile,
            ShareSymbol = "🟩"
        };
    }

    private sealed class FixedUtcDateProvider : IUtcDateProvider
    {
        public FixedUtcDateProvider(DateOnly todayUtc)
        {
            TodayUtc = todayUtc;
        }

        public DateOnly TodayUtc { get; }
    }

    private sealed class MemoryStorageService : IStorageService
    {
        private readonly Dictionary<string, string> _values = new();

        public Task SaveAsync<T>(string key, T value)
        {
            _values[key] = JsonSerializer.Serialize(value);
            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string key)
        {
            if (!_values.TryGetValue(key, out string? value))
            {
                return Task.FromResult<T?>(default);
            }

            try
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(value));
            }
            catch (JsonException)
            {
                return Task.FromResult<T?>(default);
            }
        }

        public Task RemoveAsync(string key)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetRawAsync(string key, string value)
        {
            _values[key] = value;
            return Task.CompletedTask;
        }
    }
}
