namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;
using Needle.Core.Search;

internal sealed class SearchFilterController
{
    private readonly LogSearchCoordinator _search = new();
    private readonly LogFilterCoordinator _filter = new();
    private readonly CancellationSlot _filterCancellation = new();
    private readonly int _filterResultLimit;

    public SearchFilterController(int filterResultLimit)
    {
        _filterResultLimit = filterResultLimit;
    }

    public void Cancel()
    {
        _search.Cancel();
        _filterCancellation.CancelAndDispose();
    }

    public bool CanSearch(bool isDocumentOpen, string searchText)
    {
        return _search.CanSearch(isDocumentOpen, searchText);
    }

    public void ResetSearch(SearchFilterViewModel state)
    {
        _search.Cancel();
        state.SearchMatches.Clear();
        state.SearchStatus = _search.GetPendingStatus(state.SearchText);
    }

    public LogSearchMatch? GetNextMatch(SearchFilterViewModel state, long currentLine)
    {
        return _search.GetNextMatch(state.SearchMatches, currentLine);
    }

    public LogSearchMatch? GetPreviousMatch(SearchFilterViewModel state, long currentLine)
    {
        return _search.GetPreviousMatch(state.SearchMatches, currentLine);
    }

    public string FormatMatchStatus(SearchFilterViewModel state, LogSearchMatch match)
    {
        return _search.FormatMatchStatus(state.SearchMatches, match);
    }

    public async Task RunSearchAsync(LogDocumentSession session, SearchFilterViewModel state)
    {
        var cancellationToken = _search.ResetCancellation();
        state.SearchStatus = LogStatusFormatter.Searching;

        try
        {
            state.SearchStatus = await _search.LoadMatchesAsync(
                session,
                state.SearchText,
                state.IsSearchCaseSensitive,
                state.IsSearchRegex,
                state.SearchMatches,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            state.SearchStatus = LogStatusFormatter.SearchCancelled;
        }
        catch (Exception ex)
        {
            state.SearchStatus = LogStatusFormatter.SearchFailed(ex);
        }
    }

    public async Task ApplyFilterAsync(
        LogDocumentSession session,
        SearchFilterViewModel state,
        IReadOnlyList<string> excludePatterns)
    {
        var cancellationToken = _filterCancellation.Reset().Token;

        state.FilteredLineNumbers.Clear();
        state.FilterStatus = LogStatusFormatter.Filtering;

        try
        {
            if (!_filter.HasVisibilityFilter(state.FilterText, excludePatterns))
            {
                ClearFilter(state);
                return;
            }

            await foreach (var lineNumber in _filter.GetVisibleLineNumbersAsync(
                               session,
                               state.FilterText,
                               excludePatterns,
                               cancellationToken))
            {
                state.FilteredLineNumbers.Add(lineNumber);
                if (state.FilteredLineNumbers.Count >= _filterResultLimit)
                {
                    state.FilterStatus = LogStatusFormatter.FirstFilteredLines(_filterResultLimit);
                    break;
                }
            }

            state.IsFilterActive = true;
            if (state.FilteredLineNumbers.Count == 0)
            {
                state.FilterStatus = LogStatusFormatter.NoFilteredLines;
            }
            else if (state.FilteredLineNumbers.Count < _filterResultLimit)
            {
                state.FilterStatus = LogStatusFormatter.FilteredLines(state.FilteredLineNumbers.Count);
            }
        }
        catch (OperationCanceledException)
        {
            state.FilterStatus = LogStatusFormatter.FilterCancelled;
        }
        catch (Exception ex)
        {
            state.FilterStatus = LogStatusFormatter.FilterFailed(ex);
        }
    }

    public void ClearFilter(SearchFilterViewModel state)
    {
        _filterCancellation.CancelAndDispose();
        state.FilteredLineNumbers.Clear();
        state.IsFilterActive = false;
        state.FilterStatus = string.Empty;
    }
}
