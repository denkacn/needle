namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

internal static class LogTabStateMapper
{
    public static LogTabTransientState Capture(
        long fileSize,
        double scrollLine,
        string searchText,
        bool isSearchCaseSensitive,
        bool isSearchRegex,
        string searchStatus,
        IEnumerable<LogSearchMatch> searchMatches,
        string filterText,
        string filterStatus,
        bool isFilterActive,
        IEnumerable<long> filteredLineNumbers,
        IEnumerable<string> excludePatterns,
        IEnumerable<string> triggerPatterns,
        string excludeStatus,
        IEnumerable<LogBookmark> bookmarks,
        string bookmarkStatus,
        bool isTailPaused,
        bool isFollowingTail,
        bool isStructuredMode,
        long selectedLineNumber,
        string selectedLineText)
    {
        return new LogTabTransientState(
            fileSize,
            scrollLine,
            searchText,
            isSearchCaseSensitive,
            isSearchRegex,
            searchStatus,
            [.. searchMatches],
            filterText,
            filterStatus,
            isFilterActive,
            [.. filteredLineNumbers],
            [.. excludePatterns],
            [.. triggerPatterns],
            excludeStatus,
            [.. bookmarks],
            bookmarkStatus,
            isTailPaused,
            isFollowingTail,
            isStructuredMode,
            selectedLineNumber,
            selectedLineText);
    }

    public static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
