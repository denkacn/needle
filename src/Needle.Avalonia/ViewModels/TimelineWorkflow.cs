namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

internal sealed class TimelineWorkflow
{
    private readonly LogTimelineViewModel _timeline;
    private readonly DocumentStateViewModel _document;
    private readonly ObservableCollection<LogLineViewModel> _visibleLines;
    private readonly ObservableCollection<LogBookmark> _bookmarks;
    private readonly SearchFilterViewModel _searchFilter;
    private readonly Func<TriggerRulesViewModel> _getTriggers;
    private readonly Func<long, long> _clampFirstVisibleLine;

    public TimelineWorkflow(
        LogTimelineViewModel timeline,
        DocumentStateViewModel document,
        ObservableCollection<LogLineViewModel> visibleLines,
        ObservableCollection<LogBookmark> bookmarks,
        SearchFilterViewModel searchFilter,
        Func<TriggerRulesViewModel> getTriggers,
        Func<long, long> clampFirstVisibleLine)
    {
        _timeline = timeline;
        _document = document;
        _visibleLines = visibleLines;
        _bookmarks = bookmarks;
        _searchFilter = searchFilter;
        _getTriggers = getTriggers;
        _clampFirstVisibleLine = clampFirstVisibleLine;
    }

    public void Reset()
    {
        _timeline.Reset();
    }

    public void Refresh(bool captureVisibleHighlights = true)
    {
        if (captureVisibleHighlights)
        {
            _timeline.CaptureVisibleHighlights(_visibleLines);
        }

        _timeline.Refresh(
            _document.TotalLineCount,
            _bookmarks,
            _searchFilter.SearchMatches,
            _getTriggers().Hits);
    }

    public void ClearCapturedHighlights()
    {
        _timeline.ClearCapturedHighlights();
        Refresh(captureVisibleHighlights: false);
    }

    public void JumpToLine(long lineNumber)
    {
        if (!_document.IsDocumentOpen)
        {
            return;
        }

        var selectedLine = Math.Clamp(lineNumber, 0, Math.Max(0, _document.TotalLineCount - 1));
        var targetLine = _searchFilter.IsFilterActive
            ? FindNearestFilteredLineIndex(selectedLine)
            : selectedLine;

        _document.SelectedLineNumber = selectedLine + 1;
        _document.ScrollLine = _clampFirstVisibleLine(targetLine);
    }

    private long FindNearestFilteredLineIndex(long lineNumber)
    {
        if (_searchFilter.FilteredLineNumbers.Count == 0)
        {
            return 0;
        }

        var bestIndex = 0;
        var bestDistance = long.MaxValue;
        for (var i = 0; i < _searchFilter.FilteredLineNumbers.Count; i++)
        {
            var distance = Math.Abs(_searchFilter.FilteredLineNumbers[i] - lineNumber);
            if (distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestIndex = i;
            if (distance == 0)
            {
                break;
            }
        }

        return bestIndex;
    }
}
