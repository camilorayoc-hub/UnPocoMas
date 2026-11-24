using UnPocoMas.UI.Interfaces;

namespace UnPocoMas.Maui.Services;

public sealed class DummyShareService : IShareService
{
    public Task ShareTextAsync(string text)
    {
        return Task.CompletedTask;
    }
}

public sealed class DummyHapticsService : IHapticsService
{
    public void Light()
    {
    }

    public void Success()
    {
    }

    public void Error()
    {
    }
}

public sealed class DummySoundService : ISoundService
{
    public Task PlayClickAsync()
    {
        return Task.CompletedTask;
    }

    public Task PlaySuccessAsync()
    {
        return Task.CompletedTask;
    }

    public Task PlayFailAsync()
    {
        return Task.CompletedTask;
    }
}
