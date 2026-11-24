namespace UnPocoMas.UI.Interfaces;

public interface IStorageService
{
    Task SaveAsync<T>(string key, T value);

    Task<T?> LoadAsync<T>(string key);

    Task RemoveAsync(string key);
}
