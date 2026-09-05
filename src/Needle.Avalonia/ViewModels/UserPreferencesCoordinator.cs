namespace Needle.Avalonia.ViewModels;

using Needle.Application.Preferences;

internal sealed class UserPreferencesCoordinator
{
    private readonly IUserPreferencesStore? _store;
    private readonly DebouncedAsyncAction _save;
    private readonly Func<bool> _canSave;
    private readonly Func<UserPreferences> _createPreferences;

    public UserPreferencesCoordinator(
        IUserPreferencesStore? store,
        DebouncedAsyncAction save,
        Func<bool> canSave,
        Func<UserPreferences> createPreferences)
    {
        _store = store;
        _save = save;
        _canSave = canSave;
        _createPreferences = createPreferences;
    }

    public async Task<UserPreferences?> LoadAsync()
    {
        if (_store is null)
        {
            return null;
        }

        try
        {
            return await _store.LoadAsync();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public void QueueSave()
    {
        if (_store is null || !_canSave())
        {
            return;
        }

        _save.Queue(SaveAsync);
    }

    public Task SaveNowAsync()
    {
        if (_store is null || !_canSave())
        {
            return Task.CompletedTask;
        }

        var preferences = _createPreferences();
        return Task.Run(async () => await _store.SaveAsync(preferences));
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_store is null)
        {
            return;
        }

        await _store.SaveAsync(_createPreferences(), cancellationToken);
    }
}
