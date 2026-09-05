namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Application.Preferences;
using Needle.Application.Workspace;

internal sealed class MainPersistenceWorkflow
{
    private readonly DisplaySettingsViewModel _displaySettings;
    private readonly UserPreferencesCoordinator _preferences;
    private readonly RecentFilesCoordinator _recentFiles;
    private readonly WorkspaceCoordinator _workspace;
    private readonly Func<UserPreferences> _createPreferences;
    private readonly Func<WorkspaceState> _createWorkspaceState;
    private readonly Func<bool> _canSavePreferencesContext;
    private readonly Func<bool> _canSaveWorkspaceContext;
    private bool _isRestoringPreferences;
    private bool _startupPreferencesLoaded;
    private bool _startupPreferencesRestored;

    public MainPersistenceWorkflow(
        IWorkspaceStore? workspaceStore,
        IUserPreferencesStore? preferencesStore,
        UiTaskRunner tasks,
        DisplaySettingsViewModel displaySettings,
        Func<string, Task> openRecentFileAsync,
        Func<UserPreferences> createPreferences,
        Func<WorkspaceState> createWorkspaceState,
        Func<bool> canSavePreferencesContext,
        Func<bool> canSaveWorkspaceContext,
        TimeSpan saveDelay)
    {
        _displaySettings = displaySettings;
        _createPreferences = createPreferences;
        _createWorkspaceState = createWorkspaceState;
        _canSavePreferencesContext = canSavePreferencesContext;
        _canSaveWorkspaceContext = canSaveWorkspaceContext;
        _recentFiles = new RecentFilesCoordinator(openRecentFileAsync);
        _workspace = new WorkspaceCoordinator(
            workspaceStore,
            new DebouncedAsyncAction(tasks, "Workspace save failed", saveDelay),
            CanSaveWorkspace,
            () => _createWorkspaceState());
        _preferences = new UserPreferencesCoordinator(
            preferencesStore,
            new DebouncedAsyncAction(tasks, "Preferences save failed", saveDelay),
            CanSavePreferences,
            () => _createPreferences());
    }

    public ObservableCollection<RecentFileViewModel> RecentFiles => _recentFiles.Items;

    public IReadOnlyList<string> RecentFilePaths => _recentFiles.Paths;

    public bool HasRecentFiles => _recentFiles.HasItems;

    public bool StartupPreferencesLoaded => _startupPreferencesLoaded;

    public Task<WorkspaceState?> LoadWorkspaceAsync()
    {
        return _workspace.LoadAsync();
    }

    public void QueueWorkspaceSave()
    {
        _workspace.QueueSave();
    }

    public void QueuePreferencesSave()
    {
        _preferences.QueueSave();
    }

    public Task SaveSettingsNowAsync()
    {
        return _preferences.SaveNowAsync();
    }

    public void SaveMainWindowPlacement(double width, double height, double x, double y)
    {
        if (!_startupPreferencesLoaded)
        {
            return;
        }

        _displaySettings.SaveMainWindowPlacement(width, height, x, y);
    }

    public void SaveSettingsWindowSize(double width, double height)
    {
        _displaySettings.SaveSettingsWindowSize(width, height);
    }

    public async Task<bool> RestorePreferencesAsync(Action<UserPreferences> applyPreferences)
    {
        if (_startupPreferencesLoaded)
        {
            return _startupPreferencesRestored;
        }

        var preferences = await _preferences.LoadAsync();
        _startupPreferencesLoaded = true;
        if (preferences is null)
        {
            return false;
        }

        _isRestoringPreferences = true;
        try
        {
            applyPreferences(preferences);
            _startupPreferencesRestored = true;
        }
        finally
        {
            _isRestoringPreferences = false;
        }

        return true;
    }

    public bool TrackRecentFile(string path)
    {
        _recentFiles.Add(path);
        QueuePreferencesSave();
        return true;
    }

    public bool RemoveRecentFile(string path, bool queueSave = true)
    {
        if (!_recentFiles.Remove(path))
        {
            return false;
        }

        if (queueSave)
        {
            QueuePreferencesSave();
        }

        return true;
    }

    public void RestoreRecentFiles(IEnumerable<string> paths)
    {
        _recentFiles.Restore(paths);
    }

    private bool CanSavePreferences()
    {
        return !_isRestoringPreferences && _canSavePreferencesContext();
    }

    private bool CanSaveWorkspace()
    {
        return _canSaveWorkspaceContext();
    }
}
