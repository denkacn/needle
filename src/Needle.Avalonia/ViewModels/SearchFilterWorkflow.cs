namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class SearchFilterWorkflow
{
    private readonly SearchFilterController _searchFilter;

    public SearchFilterWorkflow(int filterResultLimit)
    {
        _searchFilter = new SearchFilterController(filterResultLimit);
    }

    public void Cancel()
    {
        _searchFilter.Cancel();
    }

    public void OnSearchTextChanged(
        SearchFilterViewModel searchFilter,
        Action notifyStateChanged,
        Action notifySearchCommands,
        Action queueWorkspaceSave)
    {
        ResetSearch(searchFilter, notifyStateChanged, notifySearchCommands);
        queueWorkspaceSave();
    }

    public void OnSearchOptionChanged(
        SearchFilterViewModel searchFilter,
        Action notifyStateChanged,
        Action notifySearchCommands,
        Action queueWorkspaceSave)
    {
        ResetSearch(searchFilter, notifyStateChanged, notifySearchCommands);
        queueWorkspaceSave();
    }

    public void OnFilterTextChanged(
        SearchFilterViewModel searchFilter,
        string value,
        Action notifyClearFilterCanExecuteChanged,
        Action queueWorkspaceSave)
    {
        searchFilter.FilterStatus = string.IsNullOrWhiteSpace(value) ? string.Empty : LogStatusFormatter.PressApply;
        notifyClearFilterCanExecuteChanged();
        queueWorkspaceSave();
    }

    public async Task SearchNextAsync(
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        Func<long, long> clampFirstVisibleLine,
        Action notifyStateChanged,
        Action notifyPreviousCanExecuteChanged)
    {
        if (!CanSearch(document, searchFilter))
        {
            return;
        }

        if (searchFilter.SearchMatches.Count == 0)
        {
            await RunSearchAsync(session, searchFilter, notifyStateChanged, notifyPreviousCanExecuteChanged);
        }

        if (searchFilter.SearchMatches.Count == 0)
        {
            return;
        }

        var next = _searchFilter.GetNextMatch(searchFilter, ToLineNumber(document.ScrollLine));
        if (next is null)
        {
            return;
        }

        document.ScrollLine = clampFirstVisibleLine(next.LineNumber);
        searchFilter.SearchStatus = _searchFilter.FormatMatchStatus(searchFilter, next);
    }

    public void SearchPrevious(
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        Func<long, long> clampFirstVisibleLine)
    {
        var previous = _searchFilter.GetPreviousMatch(searchFilter, ToLineNumber(document.ScrollLine));
        if (previous is null)
        {
            return;
        }

        document.ScrollLine = clampFirstVisibleLine(previous.LineNumber);
        searchFilter.SearchStatus = _searchFilter.FormatMatchStatus(searchFilter, previous);
    }

    public bool CanSearch(DocumentStateViewModel document, SearchFilterViewModel searchFilter)
    {
        return _searchFilter.CanSearch(document.IsDocumentOpen, searchFilter.SearchText);
    }

    public async Task ApplyFilterAsync(
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        ExcludeRulesViewModel excludeRules,
        Func<long, CancellationToken, Task> loadViewportAsync,
        Action refreshMaximumScrollLine,
        Action notifyStateChanged,
        Action notifyClearFilterCanExecuteChanged,
        Action queueWorkspaceSave,
        CancellationToken cancellationToken)
    {
        if (session is null)
        {
            return;
        }

        await _searchFilter.ApplyFilterAsync(session, searchFilter, excludeRules.ToPatterns());
        notifyStateChanged();
        refreshMaximumScrollLine();
        document.ScrollLine = 0;
        await loadViewportAsync(0, cancellationToken);
        notifyClearFilterCanExecuteChanged();
        queueWorkspaceSave();
    }

    public async Task ClearFilterAsync(
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        ExcludeRulesViewModel excludeRules,
        Func<long, CancellationToken, Task> loadViewportAsync,
        Action refreshMaximumScrollLine,
        Action notifyStateChanged,
        Action notifyClearFilterCanExecuteChanged,
        Action queueWorkspaceSave,
        CancellationToken cancellationToken)
    {
        searchFilter.FilterText = string.Empty;

        if (session is not null && excludeRules.HasRules)
        {
            await _searchFilter.ApplyFilterAsync(session, searchFilter, excludeRules.ToPatterns());
        }
        else
        {
            _searchFilter.ClearFilter(searchFilter);
        }

        notifyStateChanged();
        refreshMaximumScrollLine();
        document.ScrollLine = 0;
        await loadViewportAsync(0, cancellationToken);
        notifyClearFilterCanExecuteChanged();
        queueWorkspaceSave();
    }

    private async Task RunSearchAsync(
        LogDocumentSession? session,
        SearchFilterViewModel searchFilter,
        Action notifyStateChanged,
        Action notifyPreviousCanExecuteChanged)
    {
        if (session is null)
        {
            return;
        }

        await _searchFilter.RunSearchAsync(session, searchFilter);
        notifyStateChanged();
        notifyPreviousCanExecuteChanged();
    }

    private void ResetSearch(
        SearchFilterViewModel searchFilter,
        Action notifyStateChanged,
        Action notifySearchCommands)
    {
        _searchFilter.ResetSearch(searchFilter);
        notifyStateChanged();
        notifySearchCommands();
    }

    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(value));
    }
}
