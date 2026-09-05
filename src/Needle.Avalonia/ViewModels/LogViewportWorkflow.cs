namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Application.Documents;
using Needle.Core.Bookmarks;
using Needle.Core.Highlighting;
using Needle.Parsers;

internal sealed class LogViewportWorkflow
{
    private const int MinimumViewportLineCount = 10;

    private readonly LogLinePresenter _linePresenter = new(new DefaultLogParser(), new StructuredLogFormatter());
    private readonly LogViewportCoordinator _viewport = new();

    public long ClampFirstVisibleLine(long lineNumber, DocumentStateViewModel document)
    {
        return _viewport.ClampFirstVisibleLine(lineNumber, document.MaximumScrollLine);
    }

    public void RefreshMaximumScrollLine(
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter)
    {
        document.MaximumScrollLine = _viewport.GetMaximumScrollLine(
            session,
            searchFilter.IsFilterActive,
            searchFilter.FilteredLineNumbers.Count,
            document.ViewportLineCount);
    }

    public bool SetViewportLineCount(
        double viewportHeight,
        double lineHeight,
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter)
    {
        if (viewportHeight <= 0 || lineHeight <= 0)
        {
            return false;
        }

        var visibleLines = _viewport.GetVisibleLineCount(viewportHeight, lineHeight, MinimumViewportLineCount);
        if (visibleLines == document.ViewportLineCount)
        {
            return false;
        }

        document.ViewportLineCount = visibleLines;
        RefreshMaximumScrollLine(session, document, searchFilter);

        if (document.IsDocumentOpen)
        {
            document.ScrollLine = ClampFirstVisibleLine(ToLineNumber(document.ScrollLine), document);
            return true;
        }

        return false;
    }

    public async Task LoadAsync(
        LogDocumentSession? session,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        IEnumerable<LogHighlightRule> highlightRules,
        ObservableCollection<LogLineViewModel> lines,
        Func<long, bool> isBookmarked,
        Func<long, bool> isTriggered,
        long firstLine,
        CancellationToken cancellationToken)
    {
        if (session is null)
        {
            return;
        }

        firstLine = ClampFirstVisibleLine(firstLine, document);
        if (Math.Abs(document.ScrollLine - firstLine) > 0.001)
        {
            document.ScrollLine = firstLine;
        }

        var entries = await _viewport.GetEntriesAsync(
            session,
            firstLine,
            document.ViewportLineCount,
            searchFilter.IsFilterActive,
            [.. searchFilter.FilteredLineNumbers],
            cancellationToken);

        var rules = highlightRules.ToArray();
        lines.Clear();
        foreach (var entry in entries)
        {
            lines.Add(_linePresenter.Create(
                entry,
                rules,
                isBookmarked,
                isTriggered,
                document.IsStructuredMode,
                document.SelectedLineNumber));
        }

        UpdateTimelineViewport(document, lines);
        document.HasLines = lines.Count > 0;
        var lastVisibleLine = lines.Count == 0 ? firstLine : firstLine + lines.Count;
        document.ViewportRange = lines.Count == 0 ? string.Empty : $"{firstLine + 1:N0}-{lastVisibleLine:N0}";
        document.Status = LogStatusFormatter.Viewport(
            session.LineCount,
            session.FileSize,
            document.ViewportRange,
            searchFilter.IsFilterActive,
            searchFilter.FilteredLineNumbers.Count);
    }

    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(value));
    }

    private static void UpdateTimelineViewport(DocumentStateViewModel document, IReadOnlyList<LogLineViewModel> lines)
    {
        if (lines.Count == 0)
        {
            document.TimelineScrollLine = 0;
            document.TimelineViewportLineCount = document.ViewportLineCount;
            return;
        }

        var firstLine = Math.Max(0, lines[0].LineNumber - 1);
        var lastLine = Math.Max(firstLine, lines[^1].LineNumber - 1);
        document.TimelineScrollLine = firstLine;
        document.TimelineViewportLineCount = checked((int)Math.Min(int.MaxValue, lastLine - firstLine + 1));
    }
}
