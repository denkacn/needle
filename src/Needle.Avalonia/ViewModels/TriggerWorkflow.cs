namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class TriggerWorkflow
{
    private readonly TriggerTailScanner _scanner = new();
    private readonly DocumentStateViewModel _document;
    private readonly SearchFilterViewModel _searchFilter;
    private readonly Func<TriggerRulesViewModel> _getActiveTriggers;
    private readonly Func<LogTabViewModel?> _getActiveTab;
    private readonly Func<bool> _canRefreshActiveDocument;
    private readonly Func<long, long> _clampFirstVisibleLine;
    private readonly Action<string> _setStatus;
    private readonly Action<string> _notifyPropertyChanged;
    private readonly Action _refreshTimeline;
    private readonly Action _notifyCommandsChanged;
    private readonly Action _reloadViewport;
    private readonly Action _saveWorkspace;

    public TriggerWorkflow(TriggerWorkflowContext context)
    {
        _document = context.Document;
        _searchFilter = context.SearchFilter;
        _getActiveTriggers = context.GetActiveTriggers;
        _getActiveTab = context.GetActiveTab;
        _canRefreshActiveDocument = context.CanRefreshActiveDocument;
        _clampFirstVisibleLine = context.ClampFirstVisibleLine;
        _setStatus = context.SetStatus;
        _notifyPropertyChanged = context.NotifyPropertyChanged;
        _refreshTimeline = context.RefreshTimeline;
        _notifyCommandsChanged = context.NotifyCommandsChanged;
        _reloadViewport = context.ReloadViewport;
        _saveWorkspace = context.SaveWorkspace;
    }

    public void GoToPrevious()
    {
        _getActiveTriggers().PreviousHitCommand.Execute(null);
    }

    public void GoToNext()
    {
        _getActiveTriggers().NextHitCommand.Execute(null);
    }

    public void RemoveSelected()
    {
        _getActiveTriggers().RemoveSelectedHitCommand.Execute(null);
    }

    public void Clear()
    {
        _getActiveTriggers().ClearHitsCommand.Execute(null);
    }

    public void NavigateToHit(long lineNumber)
    {
        var targetLine = lineNumber;
        if (_searchFilter.IsFilterActive)
        {
            var filteredIndex = _searchFilter.FilteredLineNumbers.IndexOf(lineNumber);
            if (filteredIndex < 0)
            {
                _setStatus("Trigger is hidden by active filter");
                return;
            }

            targetLine = filteredIndex;
        }

        _document.SelectedLineNumber = lineNumber + 1;
        _document.ScrollLine = _clampFirstVisibleLine(targetLine);
    }

    public void RefreshState()
    {
        _notifyPropertyChanged(nameof(MainViewModel.ActiveTriggers));
        _refreshTimeline();
        _notifyCommandsChanged();
        if (_canRefreshActiveDocument())
        {
            _reloadViewport();
        }

        _saveWorkspace();
    }

    public async Task ScanTailUpdateAsync(
        LogDocumentSession session,
        LogTailUpdate update,
        CancellationToken cancellationToken)
    {
        if (_getActiveTab() is not { } tab)
        {
            return;
        }

        await _scanner.ScanAsync(session, tab.Triggers, update, cancellationToken);
        _refreshTimeline();
    }
}

internal sealed record TriggerWorkflowContext(
    DocumentStateViewModel Document,
    SearchFilterViewModel SearchFilter,
    Func<TriggerRulesViewModel> GetActiveTriggers,
    Func<LogTabViewModel?> GetActiveTab,
    Func<bool> CanRefreshActiveDocument,
    Func<long, long> ClampFirstVisibleLine,
    Action<string> SetStatus,
    Action<string> NotifyPropertyChanged,
    Action RefreshTimeline,
    Action NotifyCommandsChanged,
    Action ReloadViewport,
    Action SaveWorkspace);
