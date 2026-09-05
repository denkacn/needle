using Needle.Application.Workspace;
using Needle.Core.Highlighting;

namespace Needle.Avalonia.ViewModels;

internal sealed class MainHighlightCoordinator
{
    private readonly HighlightRulesViewModel _globalRules;
    private readonly UiTaskRunner _tasks;
    private readonly Func<LogTabViewModel?> _getActiveTab;
    private readonly Func<bool> _isDocumentOpen;
    private readonly Func<Task> _reloadViewportAsync;
    private readonly Action _clearTimelineHighlights;
    private readonly Action _queuePreferencesSave;
    private readonly Action _queueWorkspaceSave;

    public MainHighlightCoordinator(
        HighlightRulesViewModel globalRules,
        UiTaskRunner tasks,
        Func<LogTabViewModel?> getActiveTab,
        Func<bool> isDocumentOpen,
        Func<Task> reloadViewportAsync,
        Action clearTimelineHighlights,
        Action queuePreferencesSave,
        Action queueWorkspaceSave)
    {
        _globalRules = globalRules ?? throw new ArgumentNullException(nameof(globalRules));
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        _getActiveTab = getActiveTab ?? throw new ArgumentNullException(nameof(getActiveTab));
        _isDocumentOpen = isDocumentOpen ?? throw new ArgumentNullException(nameof(isDocumentOpen));
        _reloadViewportAsync = reloadViewportAsync ?? throw new ArgumentNullException(nameof(reloadViewportAsync));
        _clearTimelineHighlights = clearTimelineHighlights ?? throw new ArgumentNullException(nameof(clearTimelineHighlights));
        _queuePreferencesSave = queuePreferencesSave ?? throw new ArgumentNullException(nameof(queuePreferencesSave));
        _queueWorkspaceSave = queueWorkspaceSave ?? throw new ArgumentNullException(nameof(queueWorkspaceSave));

        _globalRules.RulesChanged += OnGlobalRulesChanged;
    }

    public void Attach(LogTabViewModel tab)
    {
        tab.Highlights.RulesChanged += OnTabRulesChanged;
    }

    public void Detach(LogTabViewModel tab)
    {
        tab.Highlights.RulesChanged -= OnTabRulesChanged;
    }

    public IEnumerable<LogHighlightRule> GetActiveRules(LogTabViewModel? activeTab)
    {
        if (activeTab is not null)
        {
            foreach (var rule in activeTab.Highlights.ToRules())
            {
                yield return rule;
            }
        }

        foreach (var rule in _globalRules.ToRules())
        {
            yield return rule;
        }
    }

    public void RestoreGlobal(WorkspaceState state)
    {
        RestoreGlobal(state.HighlightRulesConfigured, state.HighlightRules);
    }

    public void RestoreGlobal(bool configured, IReadOnlyCollection<WorkspaceHighlightRule> rules)
    {
        if (configured)
        {
            _globalRules.Restore(configured, rules);
        }
    }

    public void RestoreActiveTab(LogTabViewModel? activeTab, bool configured, IEnumerable<WorkspaceHighlightRule> rules)
    {
        activeTab?.RestoreHighlightRules(configured, rules);
    }

    private void OnGlobalRulesChanged(object? sender, EventArgs e)
    {
        ReloadViewportIfOpen();
        _queuePreferencesSave();
    }

    private void OnTabRulesChanged(object? sender, EventArgs e)
    {
        if (sender is HighlightRulesViewModel rules
            && _getActiveTab() is LogTabViewModel activeTab
            && ReferenceEquals(activeTab.Highlights, rules))
        {
            ReloadViewportIfOpen();
        }

        _queueWorkspaceSave();
    }

    private void ReloadViewportIfOpen()
    {
        if (_isDocumentOpen())
        {
            _clearTimelineHighlights();
            _tasks.Run(_reloadViewportAsync, "Viewport refresh failed");
        }
    }
}
