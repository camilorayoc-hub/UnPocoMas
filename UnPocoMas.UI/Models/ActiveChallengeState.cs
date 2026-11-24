using UnPocoMas.Core.Models;

namespace UnPocoMas.UI.Models;

public sealed class ActiveChallengeState
{
    public DailyChallenge? Challenge { get; set; }

    public List<RoundResult> Results { get; } = new();

    public int CurrentRoundIndex { get; set; }

    public bool IsCompleted { get; set; }
}
