namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

public sealed class LogTimelineViewModel : ViewModelBase
{
    private readonly SortedSet<long> _knownErrorLines = [];
    private readonly SortedSet<long> _knownWarningLines = [];
    private readonly SortedDictionary<long, string> _knownHighlightLines = [];
    private long _totalLineCount;

    public ObservableCollection<LogTimelineMarker> Markers { get; } = [];

    public long TotalLineCount
    {
        get => _totalLineCount;
        private set => SetProperty(ref _totalLineCount, value);
    }

    public void Reset()
    {
        ClearCapturedHighlights();
        TotalLineCount = 0;
        Markers.Clear();
    }

    public void ClearCapturedHighlights()
    {
        _knownErrorLines.Clear();
        _knownWarningLines.Clear();
        _knownHighlightLines.Clear();
    }

    public void CaptureVisibleHighlights(IEnumerable<LogLineViewModel> lines)
    {
        foreach (var line in lines)
        {
            var zeroBasedLineNumber = line.LineNumber - 1;
            if (zeroBasedLineNumber < 0)
            {
                continue;
            }

            _knownErrorLines.Remove(zeroBasedLineNumber);
            _knownWarningLines.Remove(zeroBasedLineNumber);
            _knownHighlightLines.Remove(zeroBasedLineNumber);

            var kind = DetectIssueKind(line.Text);
            if (kind == LogTimelineMarkerKind.Error)
            {
                _knownErrorLines.Add(zeroBasedLineNumber);
            }
            else if (kind == LogTimelineMarkerKind.Warning)
            {
                _knownWarningLines.Add(zeroBasedLineNumber);
            }
            else if (TryGetHighlightColor(line, out var color))
            {
                _knownHighlightLines[zeroBasedLineNumber] = color;
            }
        }
    }

    public void Refresh(
        long totalLineCount,
        IEnumerable<LogBookmark> bookmarks,
        IEnumerable<LogSearchMatch> searchMatches,
        IEnumerable<TriggerHitViewModel> triggerHits)
    {
        TotalLineCount = Math.Max(0, totalLineCount);
        var markers = new List<LogTimelineMarker>();
        foreach (var line in _knownHighlightLines)
        {
            markers.Add(new LogTimelineMarker(line.Key, LogTimelineMarkerKind.Highlight, line.Value));
        }

        AddMarkers(markers, _knownWarningLines, LogTimelineMarkerKind.Warning);
        AddMarkers(markers, _knownErrorLines, LogTimelineMarkerKind.Error);
        AddMarkers(markers, searchMatches.Select(match => match.LineNumber), LogTimelineMarkerKind.Search);
        AddMarkers(markers, bookmarks.Select(bookmark => bookmark.LineNumber), LogTimelineMarkerKind.Bookmark);
        AddMarkers(markers, triggerHits.Select(hit => hit.LineNumber), LogTimelineMarkerKind.Trigger);

        Markers.Clear();
        foreach (var marker in markers
            .Where(marker => marker.LineNumber >= 0)
            .Distinct()
            .OrderBy(marker => marker.LineNumber)
            .ThenBy(marker => marker.Kind))
        {
            Markers.Add(marker);
        }
    }

    private static void AddMarkers(
        ICollection<LogTimelineMarker> markers,
        IEnumerable<long> lineNumbers,
        LogTimelineMarkerKind kind)
    {
        foreach (var lineNumber in lineNumbers)
        {
            markers.Add(new LogTimelineMarker(lineNumber, kind));
        }
    }

    private static LogTimelineMarkerKind? DetectIssueKind(string text)
    {
        if (text.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("FATAL", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase))
        {
            return LogTimelineMarkerKind.Error;
        }

        if (text.Contains("WARN", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("WARNING", StringComparison.OrdinalIgnoreCase))
        {
            return LogTimelineMarkerKind.Warning;
        }

        return null;
    }

    private static bool TryGetHighlightColor(LogLineViewModel line, out string color)
    {
        color = line.Background ?? line.Foreground ?? string.Empty;
        return !string.IsNullOrWhiteSpace(color);
    }
}
