using UnPocoMas.UI.Services;

namespace UnPocoMas.Maui.Services;

public sealed class MauiAppLifecycleService : IAppLifecycleService
{
    public event Action? Backgrounded;

    public event Action? Resumed;

    public void NotifyBackgrounded()
    {
        Backgrounded?.Invoke();
    }

    public void NotifyResumed()
    {
        Resumed?.Invoke();
    }
}
