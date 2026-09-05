namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

public sealed class TimelineWorkflowTests
{
    [Fact]
    public void JumpToLineUsesNearestFilteredLineWhenFilterIsActive()
    {
        var document = new DocumentStateViewModel
        {
            IsDocumentOpen = true,
            TotalLineCount = 100
        };
        var searchFilter = new SearchFilterViewModel { IsFilterActive = true };
        searchFilter.FilteredLineNumbers.Add(10);
        searchFilter.FilteredLineNumbers.Add(40);
        searchFilter.FilteredLineNumbers.Add(90);
        var workflow = new TimelineWorkflow(
            new LogTimelineViewModel(),
            document,
            [],
            [],
            searchFilter,
            () => new TriggerRulesViewModel(),
            line => line);

        workflow.JumpToLine(42);

        Assert.Equal(43, document.SelectedLineNumber);
        Assert.Equal(1, document.ScrollLine);
    }

    [Fact]
    public void RefreshPublishesBookmarksSearchAndTriggers()
    {
        var document = new DocumentStateViewModel { TotalLineCount = 20 };
        var timeline = new LogTimelineViewModel();
        var searchFilter = new SearchFilterViewModel();
        searchFilter.SearchMatches.Add(new LogSearchMatch(5, 0, 3));
        var triggers = new TriggerRulesViewModel();
        triggers.Hits.Add(new TriggerHitViewModel(7, "needle", "line"));
        var workflow = new TimelineWorkflow(
            timeline,
            document,
            [new LogLineViewModel(3, "WARN hot path")],
            [new LogBookmark(4)],
            searchFilter,
            () => triggers,
            line => line);

        workflow.Refresh();

        Assert.Contains(new LogTimelineMarker(2, LogTimelineMarkerKind.Warning), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(4, LogTimelineMarkerKind.Bookmark), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(5, LogTimelineMarkerKind.Search), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(7, LogTimelineMarkerKind.Trigger), timeline.Markers);
    }
}
