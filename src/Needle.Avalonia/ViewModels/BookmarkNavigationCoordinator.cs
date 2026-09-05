namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;

internal sealed class BookmarkNavigationCoordinator
{
    public long GetCurrentTopActualLineNumber(double scrollLine, bool isFilterActive, IReadOnlyList<long> filteredLineNumbers)
    {
        var topLine = ToLineNumber(scrollLine);
        if (!isFilterActive || filteredLineNumbers.Count == 0)
        {
            return topLine;
        }

        var index = (int)Math.Min(topLine, filteredLineNumbers.Count - 1);
        return filteredLineNumbers[index];
    }

    public long? GetSelectedActualLineNumber(long selectedLineNumber)
    {
        return selectedLineNumber <= 0 ? null : selectedLineNumber - 1;
    }

    public BookmarkScrollTarget GetScrollTarget(long lineNumber, bool isFilterActive, Collection<long> filteredLineNumbers)
    {
        if (!isFilterActive)
        {
            return new BookmarkScrollTarget(lineNumber, IsVisible: true);
        }

        var filteredIndex = filteredLineNumbers.IndexOf(lineNumber);
        return filteredIndex < 0
            ? new BookmarkScrollTarget(0, IsVisible: false)
            : new BookmarkScrollTarget(filteredIndex, IsVisible: true);
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

internal readonly record struct BookmarkScrollTarget(long LineNumber, bool IsVisible);
