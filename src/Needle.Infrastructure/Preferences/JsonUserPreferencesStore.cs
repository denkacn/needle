namespace Needle.Infrastructure.Preferences;

using Needle.Application.Preferences;
using Needle.Infrastructure.Persistence;

public sealed class JsonUserPreferencesStore : IUserPreferencesStore
{
    private readonly JsonFileStore<UserPreferences> _store;

    public JsonUserPreferencesStore()
        : this(GetDefaultPath())
    {
    }

    public JsonUserPreferencesStore(string path)
    {
        _store = new JsonFileStore<UserPreferences>(path);
    }

    public static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Needle", "preferences.json");
    }

    public async ValueTask<UserPreferences?> LoadAsync(CancellationToken cancellationToken = default)
    {
        return await _store.LoadAsync(cancellationToken);
    }

    public async ValueTask SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        await _store.SaveAsync(preferences, cancellationToken);
    }
}
