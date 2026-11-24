using System.Globalization;
using UnPocoMas.Core.Models;

namespace UnPocoMas.Core.Services;

public sealed class ShareTextBuilder
{
    public string Build(DailyResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        string symbols = string.Concat(
            result.Rounds.Select(round => round.ShareSymbol));
        string pointsMultiplier =
            (result.TotalPoints / 100.0).ToString("0.0", CultureInfo.InvariantCulture);
        string efficiency =
            result.EfficiencyPercent.ToString("0", CultureInfo.InvariantCulture);

        return string.Join(
            Environment.NewLine,
            $"Un Poco Más #{result.ChallengeNumber}",
            string.Empty,
            $"{symbols}  {pointsMultiplier}x",
            string.Empty,
            $"Eficiencia {efficiency} %");
    }
}
