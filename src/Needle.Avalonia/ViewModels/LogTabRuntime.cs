using Needle.Application.Documents;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

namespace Needle.Avalonia.ViewModels;

internal sealed class LogTabRuntime
{
    public LogTabRuntime(LogDocumentSession session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public LogDocumentSession Session { get; }

    public long FileSize
    {
        get => Session.FileSize;
        set => Session.FileSize = value;
    }

    public double ScrollLine { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool IsSearchCaseSensitive { get; set; }
    public bool IsSearchRegex { get; set; }
    public string SearchStatus { get; set; } = string.Empty;
    public List<LogSearchMatch> SearchMatches { get; } = [];
    public string FilterText { get; set; } = string.Empty;
    public string FilterStatus { get; set; } = string.Empty;
    public bool IsFilterActive { get; set; }
    public List<long> FilteredLineNumbers { get; } = [];
    public List<string> ExcludePatterns { get; } = [];
    public List<string> TriggerPatterns { get; } = [];
    public string ExcludeStatus { get; set; } = string.Empty;
    public List<LogBookmark> Bookmarks { get; } = [];
    public string BookmarkStatus { get; set; } = string.Empty;
    public bool IsTailPaused { get; set; }
    public bool IsFollowingTail { get; set; } = true;
    public bool IsStructuredMode { get; set; }
    public long SelectedLineNumber { get; set; }
    public string SelectedLineText { get; set; } = string.Empty;

    public void Save(LogTabTransientState state)
    {
        FileSize = state.FileSize;
        ScrollLine = state.ScrollLine;
        SearchText = state.SearchText;
        IsSearchCaseSensitive = state.IsSearchCaseSensitive;
        IsSearchRegex = state.IsSearchRegex;
        SearchStatus = state.SearchStatus;
        SearchMatches.Clear();
        SearchMatches.AddRange(state.SearchMatches);
        FilterText = state.FilterText;
        FilterStatus = state.FilterStatus;
        IsFilterActive = state.IsFilterActive;
        FilteredLineNumbers.Clear();
        FilteredLineNumbers.AddRange(state.FilteredLineNumbers);
        ExcludePatterns.Clear();
        ExcludePatterns.AddRange(state.ExcludePatterns);
        TriggerPatterns.Clear();
        TriggerPatterns.AddRange(state.TriggerPatterns);
        ExcludeStatus = state.ExcludeStatus;
        Bookmarks.Clear();
        foreach (var bookmark in state.Bookmarks.OrderBy(bookmark => bookmark.LineNumber))
        {
            Bookmarks.Add(bookmark);
        }

        BookmarkStatus = state.BookmarkStatus;
        IsTailPaused = state.IsTailPaused;
        IsFollowingTail = state.IsFollowingTail;
        IsStructuredMode = state.IsStructuredMode;
        SelectedLineNumber = state.SelectedLineNumber;
        SelectedLineText = state.SelectedLineText;
    }
}
