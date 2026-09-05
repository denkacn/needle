namespace Needle.Avalonia.ViewModels;

using Needle.Application.Workspace;
using Needle.Core.Bookmarks;

internal static class WorkspaceStateMapper
{
    public static WorkspaceState Create(
        string? activeDocumentPath,
        IEnumerable<LogTabViewModel> tabs)
    {
        return new WorkspaceState
        {
            ActiveDocumentPath = activeDocumentPath,
            Documents = [.. tabs.Select(ToDocumentState)]
        };
    }

    private static WorkspaceDocumentState ToDocumentState(LogTabViewModel tab)
    {
        return new WorkspaceDocumentState
        {
            Path = tab.Path,
            FirstVisibleLine = ToLineNumber(tab.ScrollLine),
            SearchText = tab.SearchText,
            FilterText = tab.FilterText,
            ExcludePatterns = [.. tab.ExcludePatterns],
            TriggerPatterns = [.. tab.Triggers.ToPatterns()],
            HighlightRulesConfigured = true,
            HighlightRules = tab.Highlights.ToWorkspaceRules(),
            IsFilterActive = tab.IsFilterActive,
            IsTailPaused = tab.IsTailPaused,
            IsFollowingTail = tab.IsFollowingTail,
            IsStructuredMode = tab.IsStructuredMode,
            Bookmarks = [.. tab.Bookmarks.Select(bookmark => bookmark.LineNumber)]
        };
    }

    public static void ApplyBookmarks(
        IEnumerable<long> lineNumbers,
        LogBookmarkSet bookmarkSet,
        ICollection<LogBookmark> bookmarks)
    {
        bookmarkSet.Clear();
        bookmarks.Clear();

        foreach (var lineNumber in lineNumbers.Where(lineNumber => lineNumber >= 0).Distinct().Order())
        {
            bookmarkSet.Toggle(lineNumber);
            bookmarks.Add(new LogBookmark(lineNumber));
        }
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
