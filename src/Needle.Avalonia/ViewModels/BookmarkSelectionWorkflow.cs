namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Bookmarks;

internal sealed class BookmarkSelectionWorkflow
{
    private readonly BookmarkCoordinator _bookmarks = new();
    private readonly BookmarkNavigationCoordinator _bookmarkNavigation = new();
    private readonly LogSelectionCoordinator _selection = new();

    public ObservableCollection<LogBookmark> Bookmarks { get; } = [];

    public string BookmarkStatus { get; private set; } = string.Empty;

    public string SelectedLineText { get; private set; } = string.Empty;

    public bool HasBookmarks => _bookmarks.HasBookmarks;

    public string SelectedBookmarkDisplay => Bookmarks
        .FirstOrDefault(bookmark => bookmark.LineNumber == _currentBookmarkLineNumber)?.LineNumber is long lineNumber
            ? $"{lineNumber + 1:N0}"
            : string.Empty;

    private long? _currentBookmarkLineNumber;

    public bool Contains(long lineNumber)
    {
        return _bookmarks.Contains(lineNumber);
    }

    public void Toggle(
        DocumentStateViewModel document,
        bool isFilterActive,
        IReadOnlyList<long> filteredLineNumbers)
    {
        if (!document.IsDocumentOpen)
        {
            return;
        }

        _bookmarks.Toggle(
            GetSelectedActualLineNumber(document) ?? GetCurrentTopActualLineNumber(document, isFilterActive, filteredLineNumbers),
            Bookmarks);
        RefreshBookmarkState(document);
    }

    public bool RemoveCurrent(
        DocumentStateViewModel document,
        bool isFilterActive,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine)
    {
        var lineNumber = _currentBookmarkLineNumber
            ?? GetSelectedActualLineNumber(document);
        if (lineNumber is not long bookmarkLine || !_bookmarks.Contains(bookmarkLine))
        {
            return false;
        }

        _bookmarks.Remove(bookmarkLine, Bookmarks);
        RefreshBookmarkState(document);
        if (_bookmarks.HasBookmarks)
        {
            TryGoToNext(document, isFilterActive, filteredLineNumbers, clampFirstVisibleLine);
        }
        else
        {
            _currentBookmarkLineNumber = null;
        }

        return true;
    }

    public bool TryGoToNext(
        DocumentStateViewModel document,
        bool isFilterActive,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine)
    {
        return TryGoToBookmark(
            document,
            isFilterActive,
            filteredLineNumbers,
            clampFirstVisibleLine,
            _bookmarks.TryGetNext);
    }

    public bool TryGoToPrevious(
        DocumentStateViewModel document,
        bool isFilterActive,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine)
    {
        return TryGoToBookmark(
            document,
            isFilterActive,
            filteredLineNumbers,
            clampFirstVisibleLine,
            _bookmarks.TryGetPrevious);
    }

    public void Restore(IEnumerable<long> lineNumbers, DocumentStateViewModel document)
    {
        _bookmarks.Restore(lineNumbers, Bookmarks);
        RefreshBookmarkState(document);
    }

    public void RestoreStatus(string bookmarkStatus)
    {
        BookmarkStatus = bookmarkStatus;
    }

    public void Clear(DocumentStateViewModel document)
    {
        _bookmarks.Clear(Bookmarks);
        BookmarkStatus = string.Empty;
        _currentBookmarkLineNumber = null;
        RefreshBookmarkState(document);
    }

    public void ApplySelectionState(LogSelectionState state, DocumentStateViewModel document)
    {
        SelectedLineText = state.SelectedLineText;
        document.SelectedText = state.SelectedText;
    }

    public void ClearSelection(DocumentStateViewModel document)
    {
        ApplySelectionState(_selection.Clear(), document);
    }

    public void RefreshSelection(DocumentStateViewModel document, IEnumerable<LogLineViewModel> lines)
    {
        ApplySelectionState(_selection.Refresh(document.SelectedLineNumber, lines, SelectedLineText), document);
    }

    public string? GetTextToCopy(DocumentStateViewModel document)
    {
        return _selection.GetTextToCopy(document.SelectedText, SelectedLineText);
    }

    public void RestoreSelectedLineText(string selectedLineText)
    {
        SelectedLineText = selectedLineText;
    }

    public void RefreshBookmarkState(DocumentStateViewModel document)
    {
        BookmarkStatus = _bookmarks.Status;
        document.BookmarkCountText = _bookmarks.CountText;
    }

    private long GetCurrentTopActualLineNumber(
        DocumentStateViewModel document,
        bool isFilterActive,
        IReadOnlyList<long> filteredLineNumbers)
    {
        return _bookmarkNavigation.GetCurrentTopActualLineNumber(
            document.ScrollLine,
            isFilterActive,
            filteredLineNumbers);
    }

    private long? GetSelectedActualLineNumber(DocumentStateViewModel document)
    {
        return _bookmarkNavigation.GetSelectedActualLineNumber(document.SelectedLineNumber);
    }

    private bool TryGoToBookmark(
        DocumentStateViewModel document,
        bool isFilterActive,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine,
        TryGetBookmark getBookmark)
    {
        var currentLineNumber = GetSelectedActualLineNumber(document)
            ?? GetCurrentTopActualLineNumber(document, isFilterActive, filteredLineNumbers);

        for (var attempt = 0; attempt < _bookmarks.Count; attempt++)
        {
            if (!getBookmark(currentLineNumber, out var lineNumber))
            {
                break;
            }

            if (TryScrollToActualLine(lineNumber, document, isFilterActive, filteredLineNumbers, clampFirstVisibleLine))
            {
                return true;
            }

            currentLineNumber = lineNumber;
        }

        BookmarkStatus = "No visible bookmarks";
        return false;
    }

    private bool TryScrollToActualLine(
        long lineNumber,
        DocumentStateViewModel document,
        bool isFilterActive,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine)
    {
        var target = _bookmarkNavigation.GetScrollTarget(lineNumber, isFilterActive, filteredLineNumbers);
        if (!target.IsVisible)
        {
            BookmarkStatus = "Bookmark is hidden by active filter";
            return false;
        }

        document.SelectedLineNumber = lineNumber + 1;
        document.ScrollLine = clampFirstVisibleLine(target.LineNumber);
        _currentBookmarkLineNumber = lineNumber;
        return true;
    }

    private delegate bool TryGetBookmark(long currentLineNumber, out long lineNumber);
}
