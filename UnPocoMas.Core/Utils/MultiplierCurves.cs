using UnPocoMas.Core.Configuration;
using UnPocoMas.Core.Models;

namespace UnPocoMas.Core.Utils;

public static class MultiplierCurves
{
    public static double GetCrashMultiplier(double randomValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(randomValue);

        if (randomValue >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(randomValue));
        }

        return Math.Min(
            GameConfig.CrashConstant / (1.0 - randomValue),
            GameConfig.CrashCap);
    }

    public static double GetMultiplier(
        RoundProfile profile,
        double seconds)
    {
        return profile switch
        {
            RoundProfile.Standard => Math.Exp(0.25 * seconds),

            RoundProfile.Trap => Math.Exp(0.25 * seconds),

            RoundProfile.Accelerated =>
                Math.Exp(0.15 * seconds + 0.015 * seconds * seconds),

            RoundProfile.Lightning =>
                Math.Exp(0.60 * seconds),

            RoundProfile.Final =>
                Math.Exp(0.25 * seconds),

            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
    }

    public static double GetCrashTime(
        RoundProfile profile,
        double crashMultiplier)
    {
        return profile switch
        {
            RoundProfile.Standard => Math.Log(crashMultiplier) / 0.25,

            RoundProfile.Trap => Math.Log(crashMultiplier) / 0.25,

            RoundProfile.Final => Math.Log(crashMultiplier) / 0.25,

            RoundProfile.Lightning => Math.Log(crashMultiplier) / 0.60,

            RoundProfile.Accelerated =>
                SolveAcceleratedInverse(crashMultiplier),

            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
    }

    private static double SolveAcceleratedInverse(
        double multiplier)
    {
        double ln = Math.Log(multiplier);

        double a = 0.015;
        double b = 0.15;
        double c = -ln;

        return (-b + Math.Sqrt(b * b - 4 * a * c))
            / (2 * a);
    }
}
