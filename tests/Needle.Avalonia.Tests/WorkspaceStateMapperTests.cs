namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Core.Bookmarks;

public sealed class WorkspaceStateMapperTests
{
    [Fact]
    public void ApplyBookmarksSortsDeduplicatesAndSkipsNegativeLines()
    {
        var bookmarkSet = new LogBookmarkSet();
        var bookmarks = new List<LogBookmark>();

        WorkspaceStateMapper.ApplyBookmarks([5, -1, 2, 5], bookmarkSet, bookmarks);

        Assert.Collection(
            bookmarks,
            bookmark => Assert.Equal(2, bookmark.LineNumber),
            bookmark => Assert.Equal(5, bookmark.LineNumber));
        Assert.True(bookmarkSet.Contains(2));
        Assert.True(bookmarkSet.Contains(5));
        Assert.DoesNotContain(bookmarks, bookmark => bookmark.LineNumber < 0);
    }
}
