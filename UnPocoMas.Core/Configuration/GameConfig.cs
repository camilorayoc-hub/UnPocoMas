namespace UnPocoMas.Core.Configuration;

public sealed class GameConfig
{
    public const string AlgorithmVersion = "v1";

    public static readonly DateOnly LaunchDate =
        new(2026, 1, 1);

    public const int FirstChallengeNumber = 1;

    public const double CrashConstant = 1.30;

    public const double CrashCap = 50.0;

    public const int BasePoints = 100;

    public const int FinalRoundBasePoints = 200;

    public const double SuccessLatencySeconds = 0.050;
}
