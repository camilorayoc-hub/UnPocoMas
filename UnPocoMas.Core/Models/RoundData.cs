namespace UnPocoMas.Core.Models;

public sealed class RoundData
{
    public int RoundNumber { get; init; }

    public RoundProfile Profile { get; init; }

    public double CrashMultiplier { get; init; }

    public double CrashTimeSeconds { get; init; }

    public bool HasRealSignal { get; init; }

    public double? RealSignalTimeSeconds { get; init; }

    public bool HasFalseSignal { get; init; }

    public double? FalseSignalTimeSeconds { get; init; }

    public double FalseSignalDurationSeconds { get; init; }

    public int BasePoints { get; init; }
}
