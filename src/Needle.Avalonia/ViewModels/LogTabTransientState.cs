using Needle.Core.Bookmarks;
using Needle.Core.Search;

namespace Needle.Avalonia.ViewModels;

internal sealed record LogTabTransientState(
    long FileSize,
    double ScrollLine,
    string SearchText,
    bool IsSearchCaseSensitive,
    bool IsSearchRegex,
    string SearchStatus,
    IReadOnlyList<LogSearchMatch> SearchMatches,
    string FilterText,
    string FilterStatus,
    bool IsFilterActive,
    IReadOnlyList<long> FilteredLineNumbers,
    IReadOnlyList<string> ExcludePatterns,
    IReadOnlyList<string> TriggerPatterns,
    string ExcludeStatus,
    IReadOnlyList<LogBookmark> Bookmarks,
    string BookmarkStatus,
    bool IsTailPaused,
    bool IsFollowingTail,
    bool IsStructuredMode,
    long SelectedLineNumber,
    string SelectedLineText);
