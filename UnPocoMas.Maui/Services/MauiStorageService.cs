using System.Text.Json;
using UnPocoMas.UI.Interfaces;

namespace UnPocoMas.Maui.Services;

public sealed class MauiStorageService : IStorageService
{
    public Task SaveAsync<T>(string key, T value)
    {
        try
        {
            string json = JsonSerializer.Serialize(value);
            Preferences.Default.Set(key, json);
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    public Task<T?> LoadAsync<T>(string key)
    {
        try
        {
            string? json = Preferences.Default.Get<string?>(key, null);

            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult<T?>(default);
            }

            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
        catch
        {
            return Task.FromResult<T?>(default);
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            Preferences.Default.Remove(key);
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}
