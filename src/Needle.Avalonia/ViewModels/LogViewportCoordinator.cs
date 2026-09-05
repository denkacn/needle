namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;
using Needle.Core.Lines;

internal sealed class LogViewportCoordinator
{
    public int GetVisibleLineCount(double viewportHeight, double lineHeight, int minimumLineCount)
    {
        if (viewportHeight <= 0 || lineHeight <= 0)
        {
            return minimumLineCount;
        }

        return Math.Max(minimumLineCount, (int)Math.Floor(viewportHeight / lineHeight));
    }

    public long GetMaximumScrollLine(
        LogDocumentSession? session,
        bool isFilterActive,
        int filteredLineCount,
        int viewportLineCount)
    {
        if (session is null)
        {
            return 0;
        }

        var lineCount = isFilterActive ? filteredLineCount : session.LineCount;
        return Math.Max(0, lineCount - viewportLineCount);
    }

    public long ClampFirstVisibleLine(long lineNumber, double maximumScrollLine)
    {
        return Math.Clamp(lineNumber, 0, ToLineNumber(maximumScrollLine));
    }

    public async ValueTask<IReadOnlyList<LogEntry>> GetEntriesAsync(
        LogDocumentSession session,
        long firstLine,
        int viewportLineCount,
        bool isFilterActive,
        IReadOnlyList<long> filteredLineNumbers,
        CancellationToken cancellationToken)
    {
        if (!isFilterActive)
        {
            return await session.Reader.GetLinesAsync(session.Document, firstLine, viewportLineCount, cancellationToken);
        }

        var result = new List<LogEntry>(viewportLineCount);
        var startIndex = (int)Math.Min(firstLine, filteredLineNumbers.Count);
        var endIndex = Math.Min(filteredLineNumbers.Count, startIndex + viewportLineCount);

        for (var i = startIndex; i < endIndex; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await session.Reader.GetLinesAsync(session.Document, filteredLineNumbers[i], 1, cancellationToken);
            if (line.Count > 0)
            {
                result.Add(line[0]);
            }
        }

        return result;
    }

    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(value));
    }
}
