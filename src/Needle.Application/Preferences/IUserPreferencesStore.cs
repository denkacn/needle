namespace Needle.Application.Preferences;

public interface IUserPreferencesStore
{
    ValueTask<UserPreferences?> LoadAsync(CancellationToken cancellationToken = default);

    ValueTask SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default);
}
