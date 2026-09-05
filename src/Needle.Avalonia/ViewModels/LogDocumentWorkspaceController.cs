namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;

internal sealed class LogDocumentWorkspaceController
{
    private readonly LogTabStateWorkflow _tabState;
    private Action<LogTabViewModel> _attachTab = _ => { };
    private Action<LogTabViewModel> _detachTab = _ => { };

    public LogDocumentWorkspaceController(LogTabStateWorkflow tabState)
    {
        _tabState = tabState ?? throw new ArgumentNullException(nameof(tabState));
    }

    public void SetTabLifecycleHandlers(
        Action<LogTabViewModel> attachTab,
        Action<LogTabViewModel> detachTab)
    {
        _attachTab = attachTab ?? throw new ArgumentNullException(nameof(attachTab));
        _detachTab = detachTab ?? throw new ArgumentNullException(nameof(detachTab));
    }

    public ObservableCollection<LogTabViewModel> Tabs { get; } = [];

    public ActiveLogDocument ActiveDocument { get; } = new();

    public LogTabViewModel? ActiveTab => ActiveDocument.Tab;

    public bool HasActiveTab => ActiveDocument.HasTab;

    public LogTabViewModel? FindTab(string path)
    {
        return Tabs.FirstOrDefault(tab => string.Equals(tab.Path, path, StringComparison.OrdinalIgnoreCase));
    }

    public void ActivateOpenedTab(string path, LogTabViewModel tab)
    {
        ActiveDocument.Activate(path, tab);
        _attachTab(tab);
        Tabs.Add(tab);
        SetActiveTabVisualState(tab);
    }

    public void SwitchTo(LogTabViewModel tab)
    {
        ActiveDocument.SwitchTo(tab);
        SetActiveTabVisualState(tab);
    }

    public LogTabViewModel? SelectRestoredActiveTab(string? activeDocumentPath)
    {
        return FindTab(activeDocumentPath ?? string.Empty) ?? Tabs.FirstOrDefault();
    }

    public ClosedLogTabResult Close(LogTabViewModel tab)
    {
        var removedIndex = Tabs.IndexOf(tab);
        var wasActive = ActiveDocument.IsActive(tab);
        _detachTab(tab);
        Tabs.Remove(tab);

        var nextActiveTab = wasActive && Tabs.Count > 0
            ? Tabs[Math.Clamp(removedIndex, 0, Tabs.Count - 1)]
            : null;

        return new ClosedLogTabResult(wasActive, nextActiveTab);
    }

    public void Clear()
    {
        ActiveDocument.Clear();
        SetActiveTabVisualState(null);
    }

    public void SetActiveTabVisualState(LogTabViewModel? activeTab)
    {
        _tabState.SetActiveTabVisualState(Tabs, activeTab);
    }
}

internal sealed record ClosedLogTabResult(bool WasActive, LogTabViewModel? NextActiveTab);
