namespace Needle.Core.Tests;

using Needle.Core.Bookmarks;

public sealed class LogBookmarkSetTests
{
    [Fact]
    public void ToggleAddsAndRemovesLineNumber()
    {
        var bookmarks = new LogBookmarkSet();

        Assert.True(bookmarks.Toggle(42));
        Assert.True(bookmarks.Contains(42));
        Assert.Equal(1, bookmarks.Count);

        Assert.False(bookmarks.Toggle(42));
        Assert.False(bookmarks.Contains(42));
        Assert.Equal(0, bookmarks.Count);
    }

    [Fact]
    public void NextWrapsToFirstBookmark()
    {
        var bookmarks = new LogBookmarkSet();
        bookmarks.Toggle(10);
        bookmarks.Toggle(20);

        Assert.True(bookmarks.TryGetNext(20, out var next));
        Assert.Equal(10, next);
    }

    [Fact]
    public void PreviousWrapsToLastBookmark()
    {
        var bookmarks = new LogBookmarkSet();
        bookmarks.Toggle(10);
        bookmarks.Toggle(20);

        Assert.True(bookmarks.TryGetPrevious(10, out var previous));
        Assert.Equal(20, previous);
    }
}
