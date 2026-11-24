namespace UnPocoMas.Core.Models;

public sealed class RoundResult
{
    public int RoundNumber { get; init; }

    public bool Success { get; init; }

    public double RetireMultiplier { get; init; }

    public int Points { get; init; }

    public double CrashMultiplier { get; init; }

    public double CrashTimeSeconds { get; init; }

    public RoundProfile Profile { get; init; }

    public string ShareSymbol { get; init; } = string.Empty;
}
