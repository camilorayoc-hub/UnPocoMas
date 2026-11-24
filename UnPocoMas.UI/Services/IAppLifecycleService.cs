namespace UnPocoMas.UI.Services;

public interface IAppLifecycleService
{
    event Action? Backgrounded;

    event Action? Resumed;
}
