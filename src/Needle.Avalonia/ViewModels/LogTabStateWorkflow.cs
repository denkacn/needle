namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Application.Documents;
using Needle.Core.Bookmarks;

internal sealed class LogTabStateWorkflow
{
    public void SaveActiveTabState(
        LogTabViewModel? activeTab,
        LogDocumentSession? activeSession,
        string? currentFilePath,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        ExcludeRulesViewModel excludeRules,
        IEnumerable<LogBookmark> bookmarks,
        string bookmarkStatus,
        string selectedLineText)
    {
        if (activeTab is null || currentFilePath is null)
        {
            return;
        }

        activeTab.SaveTransientState(LogTabStateMapper.Capture(
            activeSession?.FileSize ?? 0,
            document.ScrollLine,
            searchFilter.SearchText,
            searchFilter.IsSearchCaseSensitive,
            searchFilter.IsSearchRegex,
            searchFilter.SearchStatus,
            [.. searchFilter.SearchMatches],
            searchFilter.FilterText,
            searchFilter.FilterStatus,
            searchFilter.IsFilterActive,
            [.. searchFilter.FilteredLineNumbers],
            excludeRules.ToPatterns(),
            activeTab.Triggers.ToPatterns(),
            excludeRules.Status,
            [.. bookmarks],
            bookmarkStatus,
            document.IsTailPaused,
            document.IsFollowingTail,
            document.IsStructuredMode,
            document.SelectedLineNumber,
            selectedLineText));
    }

    public void RestoreTabState(
        LogTabViewModel tab,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        ExcludeRulesViewModel excludeRules,
        BookmarkSelectionWorkflow bookmarks,
        Action refreshTailDisplayState)
    {
        searchFilter.SearchText = tab.SearchText;
        searchFilter.IsSearchCaseSensitive = tab.IsSearchCaseSensitive;
        searchFilter.IsSearchRegex = tab.IsSearchRegex;
        searchFilter.SearchStatus = tab.SearchStatus;
        LogTabStateMapper.Replace(searchFilter.SearchMatches, tab.SearchMatches);

        searchFilter.FilterText = tab.FilterText;
        searchFilter.FilterStatus = tab.FilterStatus;
        searchFilter.IsFilterActive = tab.IsFilterActive;
        LogTabStateMapper.Replace(searchFilter.FilteredLineNumbers, tab.FilteredLineNumbers);
        excludeRules.Restore(tab.ExcludePatterns);
        tab.Triggers.RestoreRules(tab.TriggerPatterns);

        bookmarks.Restore(tab.Bookmarks.Select(bookmark => bookmark.LineNumber), document);
        bookmarks.RestoreStatus(tab.BookmarkStatus);
        document.IsTailPaused = tab.IsTailPaused;
        document.IsFollowingTail = tab.IsFollowingTail;
        refreshTailDisplayState();
        document.IsStructuredMode = tab.IsStructuredMode;
        document.SelectedLineNumber = tab.SelectedLineNumber;
        bookmarks.RestoreSelectedLineText(tab.SelectedLineText);
        document.SelectedText = string.Empty;
    }

    public void SetActiveTabVisualState(IEnumerable<LogTabViewModel> tabs, LogTabViewModel? activeTab)
    {
        foreach (var tab in tabs)
        {
            tab.IsActive = ReferenceEquals(tab, activeTab);
        }
    }

    public void ResetVisibleDocumentState(
        DocumentStateViewModel document,
        ObservableCollection<LogLineViewModel> lines,
        SearchFilterViewModel searchFilter,
        ExcludeRulesViewModel excludeRules,
        BookmarkSelectionWorkflow bookmarks)
    {
        lines.Clear();
        searchFilter.Clear();
        excludeRules.Restore([]);
        bookmarks.Clear(document);
        document.ResetVisible();
        bookmarks.ClearSelection(document);
    }
}
