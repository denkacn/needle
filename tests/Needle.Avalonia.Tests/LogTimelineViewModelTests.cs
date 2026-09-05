namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

public sealed class LogTimelineViewModelTests
{
    [Fact]
    public void RefreshCombinesKnownHighlightsBookmarksSearchAndTriggers()
    {
        var timeline = new LogTimelineViewModel();
        timeline.CaptureVisibleHighlights(
        [
            new LogLineViewModel(2, "WARN cache is warming"),
            new LogLineViewModel(4, "ERROR failed to parse payload"),
            new LogLineViewModel(5, "custom rule matched", Foreground: "#CFE8FF"),
            new LogLineViewModel(6, "custom background matched", Background: "#112233")
        ]);

        timeline.Refresh(
            10,
            [new LogBookmark(6)],
            [new LogSearchMatch(7, 0, 4)],
            [new TriggerHitViewModel(8, "OnReceivedMessage", "trigger text")]);

        Assert.Equal(10, timeline.TotalLineCount);
        Assert.Contains(new LogTimelineMarker(1, LogTimelineMarkerKind.Warning), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(3, LogTimelineMarkerKind.Error), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(4, LogTimelineMarkerKind.Highlight, "#CFE8FF"), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(5, LogTimelineMarkerKind.Highlight, "#112233"), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(6, LogTimelineMarkerKind.Bookmark), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(7, LogTimelineMarkerKind.Search), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(8, LogTimelineMarkerKind.Trigger), timeline.Markers);
    }

    [Fact]
    public void ResetClearsMarkersAndTotalLineCount()
    {
        var timeline = new LogTimelineViewModel();
        timeline.CaptureVisibleHighlights([new LogLineViewModel(1, "FATAL boom")]);
        timeline.Refresh(5, [], [], []);

        timeline.Reset();

        Assert.Equal(0, timeline.TotalLineCount);
        Assert.Empty(timeline.Markers);
    }

    [Fact]
    public void CaptureVisibleHighlightsKeepsPreviouslyDiscoveredHighlights()
    {
        var timeline = new LogTimelineViewModel();
        timeline.CaptureVisibleHighlights([new LogLineViewModel(3, "custom", Foreground: "#CFE8FF")]);
        timeline.Refresh(10, [], [], []);

        timeline.CaptureVisibleHighlights([new LogLineViewModel(7, "another", Foreground: "#112233")]);
        timeline.Refresh(10, [], [], []);

        Assert.Contains(new LogTimelineMarker(2, LogTimelineMarkerKind.Highlight, "#CFE8FF"), timeline.Markers);
        Assert.Contains(new LogTimelineMarker(6, LogTimelineMarkerKind.Highlight, "#112233"), timeline.Markers);
    }

    [Fact]
    public void CaptureVisibleHighlightsRemovesStaleHighlightForVisibleLine()
    {
        var timeline = new LogTimelineViewModel();
        timeline.CaptureVisibleHighlights([new LogLineViewModel(3, "custom", Foreground: "#CFE8FF")]);
        timeline.Refresh(10, [], [], []);

        timeline.CaptureVisibleHighlights([new LogLineViewModel(3, "custom")]);
        timeline.Refresh(10, [], [], []);

        Assert.DoesNotContain(new LogTimelineMarker(2, LogTimelineMarkerKind.Highlight, "#CFE8FF"), timeline.Markers);
    }

    [Fact]
    public void ClearCapturedHighlightsRemovesAccumulatedHighlightMarkers()
    {
        var timeline = new LogTimelineViewModel();
        timeline.CaptureVisibleHighlights(
        [
            new LogLineViewModel(2, "WARN stale"),
            new LogLineViewModel(3, "custom", Foreground: "#CFE8FF")
        ]);
        timeline.Refresh(10, [], [], []);

        timeline.ClearCapturedHighlights();
        timeline.Refresh(10, [], [], []);

        Assert.Empty(timeline.Markers);
    }
}
