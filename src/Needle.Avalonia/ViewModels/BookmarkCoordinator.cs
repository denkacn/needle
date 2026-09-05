namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Bookmarks;

internal sealed class BookmarkCoordinator
{
    private readonly LogBookmarkSet _bookmarks = new();

    public int Count => _bookmarks.Count;

    public string CountText => _bookmarks.Count.ToString("N0");

    public string Status => _bookmarks.Count == 0 ? string.Empty : $"{_bookmarks.Count:N0} bookmarks";

    public bool HasBookmarks => _bookmarks.Count > 0;

    public bool Contains(long lineNumber)
    {
        return _bookmarks.Contains(lineNumber);
    }

    public bool Toggle(long lineNumber, ObservableCollection<LogBookmark> target)
    {
        var added = _bookmarks.Toggle(lineNumber);
        if (added)
        {
            target.Add(new LogBookmark(lineNumber));
            return true;
        }

        var existing = target.FirstOrDefault(bookmark => bookmark.LineNumber == lineNumber);
        if (existing is not null)
        {
            target.Remove(existing);
        }

        return false;
    }

    public bool TryGetNext(long currentLineNumber, out long lineNumber)
    {
        return _bookmarks.TryGetNext(currentLineNumber, out lineNumber);
    }

    public bool TryGetPrevious(long currentLineNumber, out long lineNumber)
    {
        return _bookmarks.TryGetPrevious(currentLineNumber, out lineNumber);
    }

    public bool Remove(long lineNumber, ObservableCollection<LogBookmark> target)
    {
        if (!_bookmarks.Contains(lineNumber))
        {
            return false;
        }

        _bookmarks.Toggle(lineNumber);
        var existing = target.FirstOrDefault(bookmark => bookmark.LineNumber == lineNumber);
        if (existing is not null)
        {
            target.Remove(existing);
        }

        return true;
    }

    public void Clear(ObservableCollection<LogBookmark> target)
    {
        _bookmarks.Clear();
        target.Clear();
    }

    public void Restore(IEnumerable<long> lineNumbers, ObservableCollection<LogBookmark> target)
    {
        WorkspaceStateMapper.ApplyBookmarks(lineNumbers, _bookmarks, target);
    }
}
