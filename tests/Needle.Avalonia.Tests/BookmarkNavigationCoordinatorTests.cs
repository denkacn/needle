using System.Collections.ObjectModel;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class BookmarkNavigationCoordinatorTests
{
    [Fact]
    public void GetCurrentTopActualLineNumberUsesFilteredLineWhenFilterIsActive()
    {
        var coordinator = new BookmarkNavigationCoordinator();

        var line = coordinator.GetCurrentTopActualLineNumber(1, isFilterActive: true, [10, 20, 30]);

        Assert.Equal(20, line);
    }

    [Fact]
    public void GetScrollTargetReturnsInvisibleWhenFilteredLineIsMissing()
    {
        var coordinator = new BookmarkNavigationCoordinator();

        var target = coordinator.GetScrollTarget(42, isFilterActive: true, new Collection<long> { 10, 20 });

        Assert.False(target.IsVisible);
    }

    [Fact]
    public void GetSelectedActualLineNumberConvertsVisibleOneBasedLineToZeroBasedLine()
    {
        var coordinator = new BookmarkNavigationCoordinator();

        Assert.Equal(4, coordinator.GetSelectedActualLineNumber(5));
        Assert.Null(coordinator.GetSelectedActualLineNumber(0));
    }

    [Fact]
    public void GoToNextBookmarkSelectsVisibleOneBasedLineNumber()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true
        };
        workflow.Restore([10], document);

        Assert.True(workflow.TryGoToNext(document, isFilterActive: false, [], line => line));

        Assert.Equal(11, document.SelectedLineNumber);
        Assert.Equal(10, document.ScrollLine);
    }

    [Fact]
    public void GoToNextBookmarkUsesSelectedLineAsNavigationAnchor()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            ScrollLine = 0,
            SelectedLineNumber = 11
        };
        workflow.Restore([10, 30], document);

        Assert.True(workflow.TryGoToNext(document, isFilterActive: false, [], line => line));

        Assert.Equal(31, document.SelectedLineNumber);
        Assert.Equal(30, document.ScrollLine);
    }

    [Fact]
    public void GoToNextBookmarkWrapsFromSelectedLastBookmarkToFirst()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            SelectedLineNumber = 31
        };
        workflow.Restore([10, 30], document);

        Assert.True(workflow.TryGoToNext(document, isFilterActive: false, [], line => line));

        Assert.Equal(11, document.SelectedLineNumber);
        Assert.Equal(10, document.ScrollLine);
    }

    [Fact]
    public void GoToPreviousBookmarkWrapsFromSelectedFirstBookmarkToLast()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            SelectedLineNumber = 11
        };
        workflow.Restore([10, 30], document);

        Assert.True(workflow.TryGoToPrevious(document, isFilterActive: false, [], line => line));

        Assert.Equal(31, document.SelectedLineNumber);
        Assert.Equal(30, document.ScrollLine);
    }

    [Fact]
    public void GoToNextBookmarkSkipsHiddenBookmarksAndWrapsToVisibleBookmark()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            SelectedLineNumber = 11
        };
        workflow.Restore([10, 20, 30], document);

        Assert.True(workflow.TryGoToNext(document, isFilterActive: true, [10, 30], line => line));

        Assert.Equal(31, document.SelectedLineNumber);
        Assert.Equal(1, document.ScrollLine);
    }

    [Fact]
    public void GoToHiddenBookmarkDoesNotChangeSelection()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            SelectedLineNumber = 3
        };
        workflow.Restore([10], document);

        Assert.False(workflow.TryGoToNext(document, isFilterActive: true, [20], line => line));

        Assert.Equal(3, document.SelectedLineNumber);
    }

    [Fact]
    public void RemoveCurrentBookmarkRemovesSelectedBookmarkAndMovesToNext()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            SelectedLineNumber = 11
        };
        workflow.Restore([10, 30], document);

        Assert.True(workflow.RemoveCurrent(document, isFilterActive: false, [], line => line));

        Assert.Equal([30], workflow.Bookmarks.Select(bookmark => bookmark.LineNumber));
        Assert.Equal(31, document.SelectedLineNumber);
        Assert.Equal(30, document.ScrollLine);
    }

    [Fact]
    public void ClearBookmarksClearsStatusAndCount()
    {
        var workflow = new BookmarkSelectionWorkflow();
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true
        };
        workflow.Restore([10, 30], document);

        workflow.Clear(document);

        Assert.Empty(workflow.Bookmarks);
        Assert.False(workflow.HasBookmarks);
        Assert.Equal("0", document.BookmarkCountText);
    }
}
