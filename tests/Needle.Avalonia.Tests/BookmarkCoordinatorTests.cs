namespace Needle.Avalonia.Tests;

using System.Collections.ObjectModel;
using Needle.Avalonia.ViewModels;
using Needle.Core.Bookmarks;

public sealed class BookmarkCoordinatorTests
{
    [Fact]
    public void ToggleAddsAndRemovesBookmark()
    {
        var coordinator = new BookmarkCoordinator();
        var bookmarks = new ObservableCollection<LogBookmark>();

        Assert.True(coordinator.Toggle(42, bookmarks));
        Assert.True(coordinator.Contains(42));
        Assert.Equal("1", coordinator.CountText);
        Assert.Single(bookmarks);

        Assert.False(coordinator.Toggle(42, bookmarks));
        Assert.False(coordinator.Contains(42));
        Assert.Equal("0", coordinator.CountText);
        Assert.Empty(bookmarks);
    }

    [Fact]
    public void RestoreRebuildsBookmarkCollection()
    {
        var coordinator = new BookmarkCoordinator();
        var bookmarks = new ObservableCollection<LogBookmark>();

        coordinator.Restore([2, 5, 8], bookmarks);

        Assert.Equal(3, coordinator.Count);
        Assert.Equal([2, 5, 8], bookmarks.Select(bookmark => bookmark.LineNumber));
    }
}
